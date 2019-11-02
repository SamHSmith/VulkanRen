using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using GLFW;
using static GLFW.Glfw;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Fabricor.Vulkan
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
            attachmentDescription[0].finalLayout = VkImageLayout.ColorAttachmentOptimal;

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

        static VkImageView CreateImageView(VkDevice device, VkImage image)
        {
            VkImageViewCreateInfo createInfo = VkImageViewCreateInfo.New();
            createInfo.image = image;
            createInfo.viewType = VkImageViewType.Image2D;
            createInfo.format = surfaceFormat.format;
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

            VkPipelineVertexInputStateCreateInfo vertexInput = VkPipelineVertexInputStateCreateInfo.New();
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
            Assert(vkCreateGraphicsPipelines(device, VkPipelineCache.Null, 1, &pCreateInfo, null, &pipeline));
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



            VkSemaphoreCreateInfo pCreateInfo = VkSemaphoreCreateInfo.New();

            VkSemaphore acquireSemaphore = new VkSemaphore();
            vkCreateSemaphore(device, &pCreateInfo, null, &acquireSemaphore);

            VkSemaphore releaseSemaphore = new VkSemaphore();
            vkCreateSemaphore(device, &pCreateInfo, null, &releaseSemaphore);

            VkQueue queue = VkQueue.Null;
            vkGetDeviceQueue(device, queueFamilyIndex, 0, &queue);

            VkShaderModule traingleVS = LoadShader(device, "shaders/triangle.vert.spv");
            VkShaderModule traingleFS = LoadShader(device, "shaders/triangle.frag.spv");

            VkRenderPass renderPass = CreateRenderPass(device);

            VkPipelineCache pipelineCache = VkPipelineCache.Null;//This is critcal for performance.
            VkPipelineLayout pipelineLayout = CreatePipelineLayout(device);
            VkPipeline trianglePipeline = CreatePipeline(device, pipelineCache, renderPass, traingleVS, traingleFS, pipelineLayout);

            uint swapchainImageCount = 0;
            Assert(vkGetSwapchainImagesKHR(device, swapchain, &swapchainImageCount, null));////////////IMAGES
            VkImage[] swapchainImages = new VkImage[swapchainImageCount];
            fixed (VkImage* ptr = &swapchainImages[0])
                Assert(vkGetSwapchainImagesKHR(device, swapchain, &swapchainImageCount, ptr));

            VkImageView[] swapchainImageViews = new VkImageView[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                swapchainImageViews[i] = CreateImageView(device, swapchainImages[i]);
            }
            VkFramebuffer[] frambuffers = new VkFramebuffer[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                frambuffers[i] = CreateFramebuffer(device, renderPass, swapchainImageViews[i]);
            }

            int poolId = CommandPoolManager.CreateCommandPool();

            FCommandBuffer cmdBuffer = new FCommandBuffer(device, CommandPoolManager.GetPool(poolId));

            while (!WindowShouldClose(window))
            {
                PollEvents();

                uint imageIndex = 0;

                Assert(vkAcquireNextImageKHR(device, swapchain, ulong.MaxValue, acquireSemaphore, VkFence.Null, &imageIndex));

                Assert(vkResetCommandPool(device, CommandPoolManager.GetPool(poolId), VkCommandPoolResetFlags.None));

                VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

                cmdBuffer.renderPass = renderPass;
                cmdBuffer.pipeline = trianglePipeline;
                cmdBuffer.framebuffer = frambuffers[imageIndex];

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

                Assert(vkQueueSubmit(queue, 1, &submitInfo, VkFence.Null));

                VkPresentInfoKHR presentInfoKHR = VkPresentInfoKHR.New();
                presentInfoKHR.swapchainCount = 1;
                presentInfoKHR.pSwapchains = &swapchain;
                presentInfoKHR.pImageIndices = &imageIndex;

                presentInfoKHR.waitSemaphoreCount = 1;
                presentInfoKHR.pWaitSemaphores = &releaseSemaphore;

                Assert(vkQueuePresentKHR(queue, &presentInfoKHR));

                vkDeviceWaitIdle(device);
            }

            DestroyWindow(window);
            Terminate();
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
