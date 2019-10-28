using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GLFW;
using static GLFW.Glfw;
using Vulkan;
using static Vulkan.VulkanNative;

namespace VulkanRen
{
    unsafe class Program
    {
        static int width = 640, height = 400;
        static VkDebugReportCallbackEXT debugReport = new VkDebugReportCallbackEXT();

        static void Assert(VkResult result)
        {
            if (result != VkResult.Success)
            {
                Console.Error.WriteLine($"Error: {result}");
                throw new System.Exception($"Error: {result}");
            }
        }

        static VkPhysicalDevice PickPhysicalDevice(VkInstance instance)
        {
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
                vkGetPhysicalDeviceProperties(devices[i], &props);

                /*fixed (VkPhysicalDevice* ptr = &devices[i])
                    GLFW.Vulkan.GetPhysicalDevicePresentationSupport((IntPtr)(&instance),(IntPtr)ptr,0);*/

                if (props.deviceType == VkPhysicalDeviceType.DiscreteGpu)
                {
                    Console.WriteLine($"Picking discrete GPU: {Marshal.PtrToStringUTF8((IntPtr)props.deviceName)}");
                    return devices[i];
                }
            }


            vkGetPhysicalDeviceProperties(devices[0], &props);

            Console.WriteLine($"Picking backup GPU: {Marshal.PtrToStringUTF8((IntPtr)props.deviceName)}");
            return devices[0];
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Vulkan!");
            Init();

            if(!GLFW.Vulkan.IsSupported){
                Console.Error.WriteLine("GLFW says that vulkan is not supported.");
                return;
            }

            WindowHint(Hint.ClientApi, ClientApi.None);
            Window window = CreateWindow(width, height, "Hello Vulkan!", Monitor.None, Window.None);

            VkInstance instance = CreateInstance();
            VkPhysicalDevice physicalDevice = PickPhysicalDevice(instance);

            float[] pQueuePriorities = new float[] { 1.0f };

            VkDeviceQueueCreateInfo deviceQueueCreateInfo = VkDeviceQueueCreateInfo.New();
            deviceQueueCreateInfo.queueFamilyIndex = 0;//SHORTCUT computed from queue properties
            deviceQueueCreateInfo.queueCount = 1;
            fixed (float* ptr = &(pQueuePriorities[0]))
                deviceQueueCreateInfo.pQueuePriorities = ptr;

            VkDeviceCreateInfo createInfo = VkDeviceCreateInfo.New();
            createInfo.queueCreateInfoCount = 1;
            createInfo.pQueueCreateInfos = &deviceQueueCreateInfo;

            VkDevice device = VkDevice.Null;
            Assert(vkCreateDevice(physicalDevice, &createInfo, null, &device));



            VkSurfaceKHR surface = CreateSurface(instance, window);

            while (!WindowShouldClose(window))
            {
                PollEvents();

            }

            DestroyWindow(window);
            Terminate();
        }

        static VkSurfaceKHR CreateSurface(VkInstance instance, Window window)
        {
            
        }

        private static VkInstance CreateInstance()
        {
            List<GCHandle> handles = new List<GCHandle>();

            string[] debugLayers = new string[] {
                "VK_LAYER_GOOGLE_threading",
                "VK_LAYER_LUNARG_parameter_validation",
                //"VK_LAYER_LUNARG_device_limits", Not Present?
                "VK_LAYER_LUNARG_object_tracker",
                //"VK_LAYER_LUNARG_image", Not Present?
                "VK_LAYER_LUNARG_core_validation",
                //"VK_LAYER_LUNARG_swapchain",
                "VK_LAYER_GOOGLE_unique_objects",
            };

            byte[][] pDebugLayers = new byte[debugLayers.Length][];


            byte*[] ppDebugLayerArray = new byte*[debugLayers.Length];

            for (int i = 0; i < debugLayers.Length; i++)
            {
                pDebugLayers[i] = Encoding.UTF8.GetBytes(debugLayers[i] + char.MinValue);
                GCHandle handle = GCHandle.Alloc(pDebugLayers[i]);
                handles.Add(handle);
                fixed (byte* p = &(((byte[])handle.Target)[0]))
                {
                    ppDebugLayerArray[i] = p;
                }
            }

            List<string> requiredExtensions=new List<string>();
            requiredExtensions.Add("VK_EXT_debug_report");
            requiredExtensions.AddRange(GLFW.Vulkan.GetRequiredInstanceExtensions());

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
            VkInstance instance = new VkInstance();
            fixed (byte** layers = &ppDebugLayerArray[0])
            {

                fixed (byte** extensions = &ppExtensionNamesArray[0])
                {
                    VkInstanceCreateInfo pCreateInfo = VkInstanceCreateInfo.New();
                    pCreateInfo.ppEnabledLayerNames = layers;
                    pCreateInfo.ppEnabledExtensionNames = extensions;
                    pCreateInfo.enabledLayerCount = (uint)debugLayers.Length;
                    pCreateInfo.enabledExtensionCount = (uint)extensionNames.Length;

                    Assert(vkCreateInstance(&pCreateInfo, null, &instance));

                }
            }

            foreach (var handle in handles)
            {
                handle.Free();
            }

            PFN_vkDebugReportCallbackEXT _debugCallbackFunc = DebugCallback;

            IntPtr debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);

            VkDebugReportCallbackCreateInfoEXT createInfoEXT = VkDebugReportCallbackCreateInfoEXT.New();
            createInfoEXT.pfnCallback = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);
            createInfoEXT.flags = //VkDebugReportFlagsEXT.DebugEXT | VkDebugReportFlagsEXT.ErrorEXT | VkDebugReportFlagsEXT.WarningEXT | 
            (VkDebugReportFlagsEXT)int.MaxValue;

            byte[] debugExtFnName = Encoding.UTF8.GetBytes("vkCreateDebugReportCallbackEXT" + char.MinValue);



            IntPtr createFnPtr;

            fixed (byte* namePtr = &(debugExtFnName[0]))
            {
                createFnPtr = vkGetInstanceProcAddr(instance, namePtr);
            }

            vkCreateDebugReportCallbackEXT_d createDelegate = Marshal.GetDelegateForFunctionPointer<vkCreateDebugReportCallbackEXT_d>(createFnPtr);

            VkDebugReportCallbackCreateInfoEXT* createInfoPtr = (&createInfoEXT);
            fixed (ulong* ptr = &(debugReport.Handle))
            {
                Assert(createDelegate(instance, createInfoPtr, IntPtr.Zero, out debugReport));
            }
            return instance;
        }
        internal unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(
        VkInstance instance,
        VkDebugReportCallbackCreateInfoEXT* createInfo,
        IntPtr allocatorPtr,
        out VkDebugReportCallbackEXT ret);
        public static uint DebugCallback(uint flags, VkDebugReportObjectTypeEXT objectType, ulong @object, UIntPtr location,
         int messageCode, byte* pLayerPrefix, byte* pMessage, void* pUserData)
        {
            string layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pLayerPrefix);
            string messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pMessage);

            System.Console.WriteLine("DebugReport layer: {0} message: {1}", layerString, messageString);
            return VkBool32.False;
        }


    }
}
