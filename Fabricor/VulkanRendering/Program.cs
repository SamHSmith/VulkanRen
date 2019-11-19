using System.Net.Mime;
using System.Numerics;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Threading;
using GLFW;
using static GLFW.Glfw;
using Vulkan;
using static Vulkan.VulkanNative;
using Fabricor.VulkanRendering;

namespace Fabricor.VulkanRendering
{
    unsafe class Program
    {
        public static int width = 640, height = 400;

        static void Assert(VkResult result)
        {
            if (result != VkResult.Success)
            {
                Console.Error.WriteLine($"Error: {result}");
                throw new System.Exception($"Error: {result}");
            }
        }

        static VkPhysicalDevice PickPhysicalDevice(VkInstance instance, VkSurfaceKHR surface, out uint queueFamilyIndex)
        {
            queueFamilyIndex = 0;
            uint deviceCount = 0;

            vkEnumeratePhysicalDevices(instance, &deviceCount, null);
            Console.WriteLine($"There are {deviceCount} devices available.");
            VkPhysicalDevice[] devices = new VkPhysicalDevice[deviceCount];
            if (deviceCount <= 0)
                return VkPhysicalDevice.Null;
            fixed (VkPhysicalDevice* ptr = &devices[0])
                vkEnumeratePhysicalDevices(instance, &deviceCount, ptr);
            VkPhysicalDeviceProperties props = new VkPhysicalDeviceProperties();


            for (int i = 0; i < deviceCount; i++)
            {
                bool badGpu = true;
                bool onlyPickDiscrete = true;

                vkGetPhysicalDeviceProperties(devices[i], &props);

                uint familyQueueCount = 0;
                vkGetPhysicalDeviceQueueFamilyProperties(devices[i], &familyQueueCount, null);
                VkQueueFamilyProperties[] famProps = new VkQueueFamilyProperties[familyQueueCount];
                fixed (VkQueueFamilyProperties* ptr = famProps)
                    vkGetPhysicalDeviceQueueFamilyProperties(devices[i], &familyQueueCount, ptr);

                for (uint k = 0; k < familyQueueCount; k++)
                {
                    if ((int)(famProps[k].queueFlags & VkQueueFlags.Graphics) <= 0)
                    {
                        continue;
                    }
                    VkBool32 supported = VkBool32.False;
                    Assert(vkGetPhysicalDeviceSurfaceSupportKHR(devices[i], k, surface, &supported));
                    if (supported == VkBool32.False)
                    {
                        continue;
                    }

                    /*fixed (VkPhysicalDevice* ptr = &(devices[i]))
                        if (!GLFW.Vulkan.GetPhysicalDevicePresentationSupport((IntPtr)(&instance), (IntPtr)ptr, k))//Throws exception
                        {
                            continue;
                        }*/
                    if (onlyPickDiscrete && !(props.deviceType == VkPhysicalDeviceType.DiscreteGpu))
                    {
                        continue;
                    }

                    queueFamilyIndex = k;
                    badGpu = false;
                }



                //Here we pick if its acceptable
                if (badGpu)
                    continue;


                Console.WriteLine($"Picking GPU: {Marshal.PtrToStringUTF8((IntPtr)props.deviceName)}");
                return devices[i];

            }

            throw new System.Exception("There was no GPU that filled our needs.");
        }

        static VkRenderPass CreateRenderPass(VkDevice device)
        {
            VkAttachmentDescription[] attachmentDescription = new VkAttachmentDescription[1];
            attachmentDescription[0] = new VkAttachmentDescription();
            attachmentDescription[0].format = surfaceFormat.format;
            attachmentDescription[0].samples = VkSampleCountFlags.Count1;
            attachmentDescription[0].loadOp = VkAttachmentLoadOp.Clear;
            attachmentDescription[0].storeOp = VkAttachmentStoreOp.Store;
            attachmentDescription[0].stencilLoadOp = VkAttachmentLoadOp.DontCare;
            attachmentDescription[0].stencilStoreOp = VkAttachmentStoreOp.DontCare;
            attachmentDescription[0].initialLayout = VkImageLayout.ColorAttachmentOptimal;
            attachmentDescription[0].finalLayout = VkImageLayout.PresentSrcKHR;

            VkAttachmentReference attachmentReference = new VkAttachmentReference();
            attachmentReference.attachment = 0;//refers to the index in the array above
            attachmentReference.layout = VkImageLayout.ColorAttachmentOptimal;

            VkSubpassDescription subpassDescription = new VkSubpassDescription();
            subpassDescription.pipelineBindPoint = VkPipelineBindPoint.Graphics;
            subpassDescription.colorAttachmentCount = 1;
            subpassDescription.pColorAttachments = &attachmentReference;

            VkRenderPassCreateInfo pCreateInfo = VkRenderPassCreateInfo.New();
            pCreateInfo.attachmentCount = 1;
            fixed (VkAttachmentDescription* ptr = &attachmentDescription[0])
                pCreateInfo.pAttachments = ptr;
            pCreateInfo.subpassCount = 1;
            pCreateInfo.pSubpasses = &subpassDescription;


            VkRenderPass pass = VkRenderPass.Null;
            Assert(vkCreateRenderPass(device, &pCreateInfo, null, &pass));
            return pass;
        }

        static VkFramebuffer CreateFramebuffer(VkDevice device, VkRenderPass pass, VkImageView imageView)
        {
            VkFramebufferCreateInfo createInfo = VkFramebufferCreateInfo.New();
            createInfo.renderPass = pass;
            createInfo.attachmentCount = 1;
            createInfo.pAttachments = &imageView;
            createInfo.width = (uint)width;
            createInfo.height = (uint)height;
            createInfo.layers = 1;

            VkFramebuffer framebuffer = VkFramebuffer.Null;
            Assert(vkCreateFramebuffer(device, &createInfo, null, &framebuffer));
            return framebuffer;
        }

        static VkImageView CreateImageView(VkDevice device, VkImage image,VkFormat imageFormat)
        {
            VkImageViewCreateInfo createInfo = VkImageViewCreateInfo.New();
            createInfo.image = image;
            createInfo.viewType = VkImageViewType.Image2D;
            createInfo.format = imageFormat;
            createInfo.subresourceRange.aspectMask = VkImageAspectFlags.Color;
            createInfo.subresourceRange.layerCount = 1;
            createInfo.subresourceRange.levelCount = 1;

            VkImageView view = VkImageView.Null;
            Assert(vkCreateImageView(device, &createInfo, null, &view));
            return view;
        }

        static VkShaderModule LoadShader(VkDevice device, string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            uint length = (uint)bytes.Length;

            VkShaderModuleCreateInfo pCreateInfo = VkShaderModuleCreateInfo.New();
            pCreateInfo.codeSize = (UIntPtr)length;
            fixed (byte* ptr = bytes)
                pCreateInfo.pCode = (uint*)ptr;

            VkShaderModule shaderModule = new VkShaderModule();
            Assert(vkCreateShaderModule(device, &pCreateInfo, null, &shaderModule));

            return shaderModule;
        }
        static VkPipelineLayout CreatePipelineLayout(VkDevice device)
        {
            VkPipelineLayoutCreateInfo pCreateInfo = VkPipelineLayoutCreateInfo.New();
            pCreateInfo.pushConstantRangeCount = 0;
            pCreateInfo.setLayoutCount = 0;

            VkPipelineLayout layout = VkPipelineLayout.Null;
            Assert(vkCreatePipelineLayout(device, &pCreateInfo, null, &layout));
            return layout;
        }
        static VkPipeline CreatePipeline(VkDevice device, VkPipelineCache pipelineCache, VkRenderPass renderPass,
        VkShaderModule vs, VkShaderModule fs, VkPipelineLayout layout)
        {

            VkGraphicsPipelineCreateInfo pCreateInfo = VkGraphicsPipelineCreateInfo.New();
            pCreateInfo.flags = VkPipelineCreateFlags.DisableOptimization;

            VkPipelineShaderStageCreateInfo[] shaderStages = new VkPipelineShaderStageCreateInfo[2];
            shaderStages[0] = VkPipelineShaderStageCreateInfo.New();
            shaderStages[0].stage = VkShaderStageFlags.Vertex;
            shaderStages[0].module = vs;

            byte[] vsFuncName = Encoding.UTF8.GetBytes("main" + char.MinValue);
            fixed (byte* ptr = &(vsFuncName[0]))
                shaderStages[0].pName = ptr;

            shaderStages[1] = VkPipelineShaderStageCreateInfo.New();
            shaderStages[1].stage = VkShaderStageFlags.Fragment;
            shaderStages[1].module = fs;
            byte[] fsFuncName = Encoding.UTF8.GetBytes("main" + char.MinValue);
            fixed (byte* ptr = &(fsFuncName[0]))
                shaderStages[1].pName = ptr;

            fixed (VkPipelineShaderStageCreateInfo* ptr = shaderStages)
                pCreateInfo.pStages = ptr;
            pCreateInfo.stageCount = 2;

            VkVertexInputBindingDescription[] bindings = new VkVertexInputBindingDescription[4];
            bindings[0] = new VkVertexInputBindingDescription();
            bindings[0].binding = 0;
            bindings[0].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[0].inputRate = VkVertexInputRate.Vertex;

            bindings[1] = new VkVertexInputBindingDescription();
            bindings[1].binding = 1;
            bindings[1].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[1].inputRate = VkVertexInputRate.Vertex;

            bindings[2] = new VkVertexInputBindingDescription();
            bindings[2].binding = 2;
            bindings[2].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[2].inputRate = VkVertexInputRate.Vertex;

            bindings[3] = new VkVertexInputBindingDescription();
            bindings[3].binding = 3;
            bindings[3].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[3].inputRate = VkVertexInputRate.Vertex;

            VkVertexInputAttributeDescription[] attribs = new VkVertexInputAttributeDescription[4];
            attribs[0].binding = 0;
            attribs[0].location = 0;
            attribs[0].format = VkFormat.R32g32b32Sfloat;
            attribs[0].offset = 0;

            attribs[1].binding = 1;
            attribs[1].location = 1;
            attribs[1].format = VkFormat.R32g32b32Sfloat;
            attribs[1].offset = 0;

            attribs[2].binding = 2;
            attribs[2].location = 2;
            attribs[2].format = VkFormat.R32g32b32Sfloat;
            attribs[2].offset = 0;

            attribs[3].binding = 3;
            attribs[3].location = 3;
            attribs[3].format = VkFormat.R32g32b32Sfloat;
            attribs[3].offset = 0;

            VkPipelineVertexInputStateCreateInfo vertexInput = VkPipelineVertexInputStateCreateInfo.New();
            fixed (VkVertexInputBindingDescription* ptr = bindings)
                vertexInput.pVertexBindingDescriptions = ptr;
            vertexInput.vertexBindingDescriptionCount = (uint)bindings.Length;
            fixed (VkVertexInputAttributeDescription* ptr = attribs)
                vertexInput.pVertexAttributeDescriptions = ptr;
            vertexInput.vertexAttributeDescriptionCount = (uint)attribs.Length;
            pCreateInfo.pVertexInputState = &vertexInput;

            VkPipelineInputAssemblyStateCreateInfo inputAssembly = VkPipelineInputAssemblyStateCreateInfo.New();
            inputAssembly.topology = VkPrimitiveTopology.TriangleList;
            pCreateInfo.pInputAssemblyState = &inputAssembly;

            VkPipelineViewportStateCreateInfo viewportState = VkPipelineViewportStateCreateInfo.New();
            viewportState.viewportCount = 1;
            viewportState.scissorCount = 1;
            pCreateInfo.pViewportState = &viewportState;

            VkPipelineRasterizationStateCreateInfo rasterizationState = VkPipelineRasterizationStateCreateInfo.New();
            rasterizationState.lineWidth = 1;
            pCreateInfo.pRasterizationState = &rasterizationState;

            VkPipelineMultisampleStateCreateInfo multisampleState = VkPipelineMultisampleStateCreateInfo.New();
            multisampleState.rasterizationSamples = VkSampleCountFlags.Count1;
            pCreateInfo.pMultisampleState = &multisampleState;

            VkPipelineDepthStencilStateCreateInfo depthState = VkPipelineDepthStencilStateCreateInfo.New();
            pCreateInfo.pDepthStencilState = &depthState;

            VkPipelineColorBlendAttachmentState colourAttachment = new VkPipelineColorBlendAttachmentState();
            colourAttachment.colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A;

            VkPipelineColorBlendStateCreateInfo colourState = VkPipelineColorBlendStateCreateInfo.New();
            colourState.pAttachments = &colourAttachment;
            colourState.attachmentCount = 1;
            pCreateInfo.pColorBlendState = &colourState;

            VkDynamicState[] dynamicStates = new VkDynamicState[2];
            dynamicStates[0] = VkDynamicState.Viewport;
            dynamicStates[1] = VkDynamicState.Scissor;

            VkPipelineDynamicStateCreateInfo dynamicState = VkPipelineDynamicStateCreateInfo.New();
            dynamicState.dynamicStateCount = (uint)dynamicStates.Length;
            fixed (VkDynamicState* ptr = &(dynamicStates[0]))
                dynamicState.pDynamicStates = ptr;
            pCreateInfo.pDynamicState = &dynamicState;

            pCreateInfo.layout = layout;
            pCreateInfo.renderPass = renderPass;
            pCreateInfo.subpass = 0;


            VkPipeline pipeline = VkPipeline.Null;
            Assert(vkCreateGraphicsPipelines(device, pipelineCache, 1, &pCreateInfo, null, &pipeline));
            return pipeline;
        }
        static void Main(string[] args)
        {

            Console.WriteLine($"Hello Vulkan!");
            Init();

            if (!GLFW.Vulkan.IsSupported)
            {
                Console.Error.WriteLine("GLFW says that vulkan is not supported.");
                return;
            }

            WindowHint(Hint.ClientApi, ClientApi.None);
            NativeWindow window = new GLFW.NativeWindow(width, height, "Now native!");


            FInstance finst = new FInstance();
            VkSurfaceKHR surface = CreateSurface(finst.instance, window);
            VkDevice device = CreateDevice(finst.instance, out var physicalDevice, surface, out var queueFamilyIndex);
            VkSwapchainKHR swapchain = CreateSwapchain(VkSwapchainKHR.Null, finst.instance, device, physicalDevice, surface, queueFamilyIndex);
            CommandPoolManager.Init(device, queueFamilyIndex);
            int poolId = CommandPoolManager.CreateCommandPool();


            VkSemaphoreCreateInfo pCreateInfo = VkSemaphoreCreateInfo.New();

            VkSemaphore acquireSemaphore = new VkSemaphore();
            vkCreateSemaphore(device, &pCreateInfo, null, &acquireSemaphore);

            VkSemaphore releaseSemaphore = new VkSemaphore();
            vkCreateSemaphore(device, &pCreateInfo, null, &releaseSemaphore);

            VkQueue graphicsQueue = VkQueue.Null;
            vkGetDeviceQueue(device, queueFamilyIndex, 0, &graphicsQueue);

            VkShaderModule traingleVS = LoadShader(device, "shaders/voxel.vert.spv");
            VkShaderModule traingleFS = LoadShader(device, "shaders/voxel.frag.spv");

            VkRenderPass renderPass = CreateRenderPass(device);
            VkSampler sampler=CreateSampler(device);

            VkPipelineCache pipelineCache = VkPipelineCache.Null;//This is critcal for performance.
            VkPipelineLayout pipelineLayout = CreatePipelineLayout(device);
            VkPipeline trianglePipeline = CreatePipeline(device, pipelineCache, renderPass, traingleVS, traingleFS, pipelineLayout);

            FDataBuffer<VoxelRenderer.VoxelVertex> dataBuffer = ////DATA
            new FDataBuffer<VoxelRenderer.VoxelVertex>(device, physicalDevice, 3 * 3, VkBufferUsageFlags.VertexBuffer, VkSharingMode.Exclusive);
            Span<VoxelRenderer.VoxelVertex> span = dataBuffer.Map();
            span[0].position = new Vector3(0, 0.5f, 0);
            span[0].texcoords = new Vector2(1, 0);
            span[1].position = new Vector3(0.5f, -0.5f, 0);
            span[1].texcoords = new Vector2(0, 0);
            span[2].position = new Vector3(0.3f, -0.5f, 0);
            span[2].texcoords = new Vector2(0, 1);
            span = dataBuffer.UnMap();

            VkImage tex = LoadTexture(device, physicalDevice,poolId,graphicsQueue, "res/Alex.png");
            VkImageView texView=CreateImageView(device,tex,VkFormat.R8g8b8a8Unorm);

            uint swapchainImageCount = 0;//END DATA
            Assert(vkGetSwapchainImagesKHR(device, swapchain, &swapchainImageCount, null));////////////IMAGES
            VkImage[] swapchainImages = new VkImage[swapchainImageCount];
            fixed (VkImage* ptr = &swapchainImages[0])
                Assert(vkGetSwapchainImagesKHR(device, swapchain, &swapchainImageCount, ptr));

            VkImageView[] swapchainImageViews = new VkImageView[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                swapchainImageViews[i] = CreateImageView(device, swapchainImages[i],surfaceFormat.format);
            }
            VkFramebuffer[] frambuffers = new VkFramebuffer[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                frambuffers[i] = CreateFramebuffer(device, renderPass, swapchainImageViews[i]);
            }

            FCommandBuffer cmdBuffer = new FCommandBuffer(device, CommandPoolManager.GetPool(poolId));

            double lastTime = Glfw.Time;
            int nbFrames = 0;
            while (!WindowShouldClose(window))
            {
                PollEvents();

                // Measure speed
                double currentTime = Glfw.Time;
                nbFrames++;
                if (currentTime - lastTime >= 1.0)
                { // If last prinf() was more than 1 sec ago
                  // printf and reset timer
                    Console.WriteLine($"ms/frame: {1000.0 / nbFrames}");
                    nbFrames = 0;
                    lastTime += 1.0;
                }

                //temp
                span = dataBuffer.Map();
                span[2].position -= 0.000002f * Vector3.UnitX;
                span = dataBuffer.UnMap();

                uint imageIndex = 0;

                Assert(vkAcquireNextImageKHR(device, swapchain, ulong.MaxValue, acquireSemaphore, VkFence.Null, &imageIndex));

                Assert(vkResetCommandPool(device, CommandPoolManager.GetPool(poolId), VkCommandPoolResetFlags.None));

                VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

                cmdBuffer.renderPass = renderPass;
                cmdBuffer.pipeline = trianglePipeline;
                cmdBuffer.framebuffer = frambuffers[imageIndex];
                cmdBuffer.image = swapchainImages[imageIndex];

                cmdBuffer.dataBuffer = dataBuffer;

                cmdBuffer.RecordCommandBuffer();

                VkPipelineStageFlags submitStageMask = VkPipelineStageFlags.ColorAttachmentOutput;

                VkSubmitInfo submitInfo = VkSubmitInfo.New();
                submitInfo.waitSemaphoreCount = 1;
                submitInfo.pWaitSemaphores = &acquireSemaphore;
                submitInfo.pWaitDstStageMask = &submitStageMask;
                submitInfo.commandBufferCount = 1;
                fixed (VkCommandBuffer* ptr = &(cmdBuffer.buffer))
                    submitInfo.pCommandBuffers = ptr;

                submitInfo.signalSemaphoreCount = 1;
                submitInfo.pSignalSemaphores = &releaseSemaphore;

                Assert(vkQueueSubmit(graphicsQueue, 1, &submitInfo, VkFence.Null));

                VkPresentInfoKHR presentInfoKHR = VkPresentInfoKHR.New();
                presentInfoKHR.swapchainCount = 1;
                presentInfoKHR.pSwapchains = &swapchain;
                presentInfoKHR.pImageIndices = &imageIndex;

                presentInfoKHR.waitSemaphoreCount = 1;
                presentInfoKHR.pWaitSemaphores = &releaseSemaphore;

                Assert(vkQueuePresentKHR(graphicsQueue, &presentInfoKHR));

                vkDeviceWaitIdle(device);
            }

            DestroyWindow(window);
            Terminate();
        }

        private static VkSampler CreateSampler(VkDevice device){
            VkSamplerCreateInfo createInfo=VkSamplerCreateInfo.New();
            createInfo.magFilter=VkFilter.Nearest;
            createInfo.minFilter=VkFilter.Nearest;

            createInfo.addressModeU=VkSamplerAddressMode.Repeat;
            createInfo.addressModeV=VkSamplerAddressMode.Repeat;
            createInfo.addressModeW=VkSamplerAddressMode.Repeat;

            createInfo.anisotropyEnable=VkBool32.False;
            createInfo.maxAnisotropy=1;

            createInfo.borderColor=VkBorderColor.FloatOpaqueWhite;
            createInfo.unnormalizedCoordinates=VkBool32.False;

            createInfo.compareEnable=VkBool32.False;
            createInfo.compareOp=VkCompareOp.Always;

            createInfo.mipmapMode=VkSamplerMipmapMode.Linear;
            createInfo.mipLodBias=0;
            createInfo.minLod=0;
            createInfo.maxLod=0;

            VkSampler sampler=VkSampler.Null;
            Assert(vkCreateSampler(device,&createInfo,null,&sampler));

            return sampler;
        }

        private static VkImage LoadTexture(VkDevice device, VkPhysicalDevice physicalDevice, int poolId,VkQueue queue, string path)
        {
            Bitmap bitmap = new Bitmap(System.Drawing.Image.FromFile(path));

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            Span<byte> img = new Span<byte>((void*)data.Scan0, data.Stride * data.Height);

            FDataBuffer<byte> tempBuffer = new FDataBuffer<byte>(device, physicalDevice, img.Length, VkBufferUsageFlags.TransferSrc,
            VkSharingMode.Exclusive);

            Span<byte> buffer = tempBuffer.Map();
            for (int i = 0; i < img.Length; i++)
            {
                buffer[i] = img[i];
            }
            buffer = tempBuffer.UnMap();

            VkImage texture = VkImage.Null;
            VkDeviceMemory memory = VkDeviceMemory.Null;

            VkImageCreateInfo createInfo = VkImageCreateInfo.New();
            createInfo.imageType = VkImageType.Image2D;
            createInfo.extent.width = (uint)data.Width;
            createInfo.extent.height = (uint)data.Height;
            createInfo.extent.depth = 1;
            createInfo.mipLevels = 1;
            createInfo.arrayLayers = 1;
            createInfo.format = VkFormat.R8g8b8a8Unorm;
            createInfo.tiling = VkImageTiling.Optimal;
            createInfo.initialLayout = VkImageLayout.Undefined;
            createInfo.usage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.Sampled;
            createInfo.sharingMode = VkSharingMode.Exclusive;
            createInfo.samples = VkSampleCountFlags.Count1;

            Assert(vkCreateImage(device, &createInfo, null, &texture));

            VkMemoryRequirements memoryRequirements;
            vkGetImageMemoryRequirements(device, texture, &memoryRequirements);

            VkPhysicalDeviceMemoryProperties memoryProperties = new VkPhysicalDeviceMemoryProperties();
            vkGetPhysicalDeviceMemoryProperties(physicalDevice, &memoryProperties);

            VkMemoryAllocateInfo allocateInfo = VkMemoryAllocateInfo.New();
            allocateInfo.allocationSize = memoryRequirements.size;
            allocateInfo.memoryTypeIndex = FDataBuffer<byte>.SelectMemoryType(memoryProperties, memoryRequirements.memoryTypeBits,
            VkMemoryPropertyFlags.DeviceLocal);

            Assert(vkAllocateMemory(device, &allocateInfo, null, &memory));

            vkBindImageMemory(device, texture, memory, 0);

            VkCommandBufferAllocateInfo pAllocateInfo = VkCommandBufferAllocateInfo.New();
            pAllocateInfo.commandPool = CommandPoolManager.GetPool(poolId);
            pAllocateInfo.level = VkCommandBufferLevel.Primary;
            pAllocateInfo.commandBufferCount = 1;

            VkCommandBuffer cmdBuffer = VkCommandBuffer.Null;
            Assert(vkAllocateCommandBuffers(device, &pAllocateInfo, &cmdBuffer));

            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

            Assert(vkBeginCommandBuffer(cmdBuffer, &beginInfo));

            VkImageMemoryBarrier imageMemoryBarrier=VkImageMemoryBarrier.New();
            imageMemoryBarrier.srcAccessMask=VkAccessFlags.None;
            imageMemoryBarrier.dstAccessMask=VkAccessFlags.ColorAttachmentRead|VkAccessFlags.ColorAttachmentWrite;
            imageMemoryBarrier.oldLayout=VkImageLayout.Undefined;
            imageMemoryBarrier.newLayout=VkImageLayout.TransferDstOptimal;
            imageMemoryBarrier.srcQueueFamilyIndex=VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.dstQueueFamilyIndex=VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.image=texture;
            imageMemoryBarrier.subresourceRange=new VkImageSubresourceRange(){baseMipLevel=0,levelCount=1,
            baseArrayLayer=0,layerCount=1,aspectMask=VkImageAspectFlags.Color};

            vkCmdPipelineBarrier(cmdBuffer,VkPipelineStageFlags.AllCommands,VkPipelineStageFlags.AllCommands,VkDependencyFlags.ByRegion,
            0,null,0,null,1,&imageMemoryBarrier);

            VkBufferImageCopy region=new VkBufferImageCopy();
            region.bufferOffset=0;
            region.bufferRowLength=0;
            region.bufferImageHeight=0;

            region.imageSubresource.aspectMask=VkImageAspectFlags.Color;
            region.imageSubresource.mipLevel=0;
            region.imageSubresource.baseArrayLayer=0;
            region.imageSubresource.layerCount=1;

            region.imageOffset=new VkOffset3D();
            region.imageExtent=new VkExtent3D(){width=(uint)data.Width,height=(uint)data.Height,depth=1};

            vkCmdCopyBufferToImage(cmdBuffer,tempBuffer.Buffer,texture,VkImageLayout.TransferDstOptimal,1,&region);

            imageMemoryBarrier.oldLayout=VkImageLayout.TransferDstOptimal;
            imageMemoryBarrier.newLayout=VkImageLayout.ShaderReadOnlyOptimal;

            vkCmdPipelineBarrier(cmdBuffer,VkPipelineStageFlags.AllCommands,VkPipelineStageFlags.AllCommands,VkDependencyFlags.ByRegion,
            0,null,0,null,1,&imageMemoryBarrier);

            Assert(vkEndCommandBuffer(cmdBuffer));

            VkSubmitInfo submitInfo = VkSubmitInfo.New();
            submitInfo.commandBufferCount = 1;
            submitInfo.pCommandBuffers = &cmdBuffer;

            Assert(vkQueueSubmit(queue, 1, &submitInfo, VkFence.Null));
            Assert(vkQueueWaitIdle(queue));
            vkFreeCommandBuffers(device,CommandPoolManager.GetPool(poolId),1,&cmdBuffer);

            return texture;
        }

        static void GetSwapchainFormat(VkPhysicalDevice physicalDevice, VkSurfaceKHR surface)
        {
            uint formatCount = 0;
            vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, null);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[formatCount];
            fixed (VkSurfaceFormatKHR* ptr = &formats[0])
                Assert(vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, ptr));

            for (int i = 0; i < formatCount; i++)
            {
                Console.WriteLine($"Format {formats[i].format} is available.");
            }
            surfaceFormat = formats[0];
        }
        static VkSurfaceFormatKHR surfaceFormat = new VkSurfaceFormatKHR();
        static VkSwapchainKHR CreateSwapchain(VkSwapchainKHR oldSwapchain, VkInstance instance, VkDevice device, VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface, uint queueFamilyIndex)
        {
            vkDestroySwapchainKHR(device, oldSwapchain, null);//Does nothing if oldswapchain is null

            VkSurfaceCapabilitiesKHR capabilities = new VkSurfaceCapabilitiesKHR();
            Assert(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(physicalDevice, surface, &capabilities));

            GetSwapchainFormat(physicalDevice, surface);
            VkSwapchainKHR swapchain = VkSwapchainKHR.Null;
            VkSwapchainCreateInfoKHR pCreateInfo = VkSwapchainCreateInfoKHR.New();
            pCreateInfo.surface = surface;
            pCreateInfo.minImageCount = capabilities.minImageCount;
            pCreateInfo.imageFormat = surfaceFormat.format;//SHORTCUT: Some devices might not support
            pCreateInfo.imageColorSpace = surfaceFormat.colorSpace;
            pCreateInfo.imageExtent = capabilities.currentExtent;
            pCreateInfo.imageArrayLayers = 1;
            pCreateInfo.imageUsage = VkImageUsageFlags.ColorAttachment;
            pCreateInfo.queueFamilyIndexCount = 1;
            pCreateInfo.pQueueFamilyIndices = &queueFamilyIndex;
            pCreateInfo.preTransform = VkSurfaceTransformFlagsKHR.IdentityKHR;
            pCreateInfo.compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR;
            pCreateInfo.presentMode = VkPresentModeKHR.MailboxKHR;

            Assert(vkCreateSwapchainKHR(device, &pCreateInfo, null, &swapchain));

            return swapchain;
        }
        static VkSurfaceKHR CreateSurface(VkInstance instance, NativeWindow window)
        {

            Assert((VkResult)GLFW.Vulkan.CreateWindowSurface(instance.Handle, window.Handle, IntPtr.Zero, out var ptr));

            VkSurfaceKHR surface = new VkSurfaceKHR((ulong)ptr.ToInt64());

            return surface;
        }

        private static VkDevice CreateDevice(VkInstance instance, out VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface, out uint queueFamilyIndex)
        {
            queueFamilyIndex = 0;//SHORTCUT computed from queue properties
            physicalDevice = PickPhysicalDevice(instance, surface, out queueFamilyIndex);

            List<GCHandle> handles = new List<GCHandle>();
            List<string> requiredExtensions = new List<string>();
            requiredExtensions.Add("VK_KHR_swapchain");
            string[] extensionNames = requiredExtensions.ToArray();
            byte[][] pExtensionNames = new byte[extensionNames.Length][];
            byte*[] ppExtensionNamesArray = new byte*[extensionNames.Length];

            for (int i = 0; i < pExtensionNames.Length; i++)
            {
                pExtensionNames[i] = Encoding.UTF8.GetBytes(extensionNames[i] + char.MinValue);
                GCHandle handle = GCHandle.Alloc(pExtensionNames[i]);
                handles.Add(handle);
                fixed (byte* p = &(((byte[])handle.Target)[0]))
                {
                    ppExtensionNamesArray[i] = p;
                }
            }
            VkDevice device;
            fixed (byte** extensions = &ppExtensionNamesArray[0])
            {
                float[] pQueuePriorities = new float[] { 1.0f };
                VkDeviceQueueCreateInfo deviceQueueCreateInfo = VkDeviceQueueCreateInfo.New();
                deviceQueueCreateInfo.queueFamilyIndex = queueFamilyIndex;
                deviceQueueCreateInfo.queueCount = 1;
                fixed (float* ptr = &(pQueuePriorities[0]))
                    deviceQueueCreateInfo.pQueuePriorities = ptr;

                VkDeviceCreateInfo createInfo = VkDeviceCreateInfo.New();
                createInfo.queueCreateInfoCount = 1;
                createInfo.pQueueCreateInfos = &deviceQueueCreateInfo;
                createInfo.ppEnabledExtensionNames = extensions;
                createInfo.enabledExtensionCount = (uint)extensionNames.Length;

                device = VkDevice.Null;
                Assert(vkCreateDevice(physicalDevice, &createInfo, null, &device));

                foreach (var handle in handles)
                {
                    handle.Free();
                }
            }

            return device;
        }
    }
}
