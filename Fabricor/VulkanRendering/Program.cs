using System.Numerics;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GLFW;
using static GLFW.Glfw;
using Vulkan;
using static Vulkan.VulkanNative;
using Fabricor.VulkanRendering.VoxelRenderer;

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
            NativeWindow window = new GLFW.NativeWindow(width, height, "Fabricor");
            Glfw.SetKeyCallback(window, GLFWInput.KeyCallback);

            FInstance finst = new FInstance();
            VkSurfaceKHR surface = CreateSurface(finst.instance, window);
            VkDevice device = CreateDevice(finst.instance, out var physicalDevice, surface, out var queueFamilyIndex);
            VkSwapchainKHR swapchain = CreateSwapchain(VkSwapchainKHR.Null, finst.instance, device, physicalDevice, surface, queueFamilyIndex);
            VkRenderPass renderPass = CreateRenderPass(device);

            uint swapchainImageCount = 0;
            Assert(vkGetSwapchainImagesKHR(device, swapchain, &swapchainImageCount, null));////////////IMAGES
            VkImage[] swapchainImages = new VkImage[swapchainImageCount];
            fixed (VkImage* ptr = &swapchainImages[0])
                Assert(vkGetSwapchainImagesKHR(device, swapchain, &swapchainImageCount, ptr));

            VkImageView[] swapchainImageViews = new VkImageView[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                swapchainImageViews[i] = FTexture.CreateColourImageView(device, swapchainImages[i], surfaceFormat.format);
            }
            VkFramebuffer[] frambuffers = new VkFramebuffer[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                frambuffers[i] = CreateFramebuffer(device, renderPass, swapchainImageViews[i]);
            }

            CommandPoolManager.Init(device, queueFamilyIndex);
            int poolId = CommandPoolManager.CreateCommandPool(VkCommandPoolCreateFlags.ResetCommandBuffer);


            VkSemaphoreCreateInfo pCreateInfo = VkSemaphoreCreateInfo.New();

            VkSemaphore acquireSemaphore = new VkSemaphore();
            vkCreateSemaphore(device, &pCreateInfo, null, &acquireSemaphore);

            VkSemaphore releaseSemaphore = new VkSemaphore();
            vkCreateSemaphore(device, &pCreateInfo, null, &releaseSemaphore);

            VkQueue graphicsQueue = VkQueue.Null;
            vkGetDeviceQueue(device, queueFamilyIndex, 0, &graphicsQueue);

            FTexture texture = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Linus.png", VkFormat.R8g8b8a8Unorm);
            FTexture texture1 = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Alex.png", VkFormat.R8g8b8a8Unorm);
            FTexture texture2 = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Alex2.png", VkFormat.R8g8b8a8Unorm);
            FTexture texture3 = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Alex3.png", VkFormat.R8g8b8a8Unorm);
            FTexture texture4 = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Cyan.png", VkFormat.R8g8b8a8Unorm);
            FTexture texture5 = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Victor.png", VkFormat.R8g8b8a8Unorm);
            FTexture texture6 = new FTexture(device, physicalDevice, poolId, graphicsQueue, "res/Red.png", VkFormat.R8g8b8a8Unorm);

            VkPipelineCache pipelineCache = VkPipelineCache.Null;//This is critcal for performance.
            FGraphicsPipeline trianglePipeline =
            new FGraphicsPipeline(device, pipelineCache, renderPass, "shaders/voxel", swapchainImageCount, new VkImageView[]{
                texture.imageView,
                texture1.imageView,
                texture2.imageView,
                texture3.imageView,
                texture4.imageView,
                texture5.imageView,
                texture6.imageView,
            });


            VoxelVertex[] vertices = new VoxelVertex[4];
            vertices[0].position = new Vector3(-0.5f, 0.5f, 0);
            vertices[0].texcoords = new Vector2(0, 1);
            vertices[1].position = new Vector3(0.5f, 0.5f, 0);
            vertices[1].texcoords = new Vector2(1, 1);
            vertices[2].position = new Vector3(0.5f, -0.5f, 0);
            vertices[2].texcoords = new Vector2(1, 0);
            vertices[3].position = new Vector3(-0.5f, -0.5f, 0);
            vertices[3].texcoords = new Vector2(0, 0);
            ushort[] indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            VoxelMesh mesh = new VoxelMesh(device, physicalDevice, vertices, indices);

            Action changeTexture = delegate
            {
                Span<VoxelVertex> span = mesh.vertices.Map();
                for (int j = 0; j < span.Length; j++)
                {
                    span[j].textureId++;
                }
                span = mesh.vertices.UnMap();
            };
            GLFWInput.Subscribe(Keys.F, changeTexture, InputState.Press);

            FCommandBuffer[] cmdBuffers = new FCommandBuffer[swapchainImageCount];
            VkFence[] fences = new VkFence[swapchainImageCount];
            for (int i = 0; i < swapchainImageCount; i++)
            {
                cmdBuffers[i] = new FCommandBuffer(device, poolId);

                VkFenceCreateInfo createInfo = VkFenceCreateInfo.New();
                createInfo.flags = VkFenceCreateFlags.Signaled;
                VkFence fence = VkFence.Null;
                Assert(vkCreateFence(device, &createInfo, null, &fence));
                fences[i] = fence;
            }

            double lastTime = Glfw.Time;
            int nbFrames = 0;
            while (!WindowShouldClose(window))
            {
                PollEvents();
                GLFWInput.Update();

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

                uint imageIndex = 0;

                Assert(vkAcquireNextImageKHR(device, swapchain, ulong.MaxValue, acquireSemaphore, VkFence.Null, &imageIndex));


                VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

                trianglePipeline.swapchainFramebuffer = frambuffers[imageIndex];
                trianglePipeline.swapchainImage = swapchainImages[imageIndex];
                trianglePipeline.swapchainImageIndex = imageIndex;

                trianglePipeline.mesh = mesh;

                fixed (VkFence* ptr = &(fences[imageIndex]))
                {
                    vkWaitForFences(device, 1, ptr, VkBool32.False, ulong.MaxValue);
                    vkResetFences(device,1,ptr);
                }
                cmdBuffers[imageIndex].RecordCommandBuffer(new Action<VkCommandBuffer>[]{
                    trianglePipeline.Execute,

                    });

                VkPipelineStageFlags submitStageMask = VkPipelineStageFlags.ColorAttachmentOutput;

                VkSubmitInfo submitInfo = VkSubmitInfo.New();
                submitInfo.waitSemaphoreCount = 1;
                submitInfo.pWaitSemaphores = &acquireSemaphore;
                submitInfo.pWaitDstStageMask = &submitStageMask;
                submitInfo.commandBufferCount = 1;
                fixed (VkCommandBuffer* ptr = &(cmdBuffers[imageIndex].buffer))
                    submitInfo.pCommandBuffers = ptr;

                submitInfo.signalSemaphoreCount = 1;
                submitInfo.pSignalSemaphores = &releaseSemaphore;

                Assert(vkQueueSubmit(graphicsQueue, 1, &submitInfo, fences[imageIndex]));

                VkPresentInfoKHR presentInfoKHR = VkPresentInfoKHR.New();
                presentInfoKHR.swapchainCount = 1;
                presentInfoKHR.pSwapchains = &swapchain;
                presentInfoKHR.pImageIndices = &imageIndex;

                presentInfoKHR.waitSemaphoreCount = 1;
                presentInfoKHR.pWaitSemaphores = &releaseSemaphore;

                Assert(vkQueuePresentKHR(graphicsQueue, &presentInfoKHR));
            }

            DestroyWindow(window);
            Terminate();
        }
        static readonly VkFormat[] UNORM_FORMATS ={
            VkFormat.R8g8b8a8Unorm,
            VkFormat.B8g8r8a8Unorm,
        };
        static void GetSwapchainFormat(VkPhysicalDevice physicalDevice, VkSurfaceKHR surface)
        {
            uint formatCount = 0;
            vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, null);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[formatCount];
            fixed (VkSurfaceFormatKHR* ptr = &formats[0])
                Assert(vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, ptr));



            for (int i = 0; i < formatCount; i++)
            {
                Console.WriteLine($"Format {formats[i].format} {formats[i].colorSpace} is available.");
                for (int k = 0; k < UNORM_FORMATS.Length; k++)
                {
                    if (formats[i].format == UNORM_FORMATS[k])
                    {
                        surfaceFormat = formats[i];
                        return;
                    }
                }

            }

            surfaceFormat = formats[0];//Fallback
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
