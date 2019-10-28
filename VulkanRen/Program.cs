using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GLFW;
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
            Glfw.Init();

            VkInstance instance = CreateInstance();


            VkPhysicalDevice physicalDevice = PickPhysicalDevice(instance);


            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);
            Window window = Glfw.CreateWindow(width, height, "Hello Vulkan!", Monitor.None, Window.None);
            while (!Glfw.WindowShouldClose(window))
            {
                Glfw.PollEvents();

            }

            Glfw.DestroyWindow(window);
        }

        private static VkInstance CreateInstance()
        {
            List<GCHandle> handles = new List<GCHandle>();

            string[] debugLayers = new string[] {
                "VK_LAYER_LUNARG_core_validation"
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

            string[] extensionNames = new string[] { "VK_EXT_debug_report", "VK_KHR_surface" };

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

            vkEnumeratePhysicalDevices(instance, null, null);

            PFN_vkDebugReportCallbackEXT _debugCallbackFunc = DebugCallback;

            IntPtr debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);

            VkDebugReportCallbackCreateInfoEXT createInfoEXT = VkDebugReportCallbackCreateInfoEXT.New();
            createInfoEXT.pfnCallback = Marshal.GetFunctionPointerForDelegate(_debugCallbackFunc);
            createInfoEXT.flags = VkDebugReportFlagsEXT.DebugEXT | VkDebugReportFlagsEXT.ErrorEXT | VkDebugReportFlagsEXT.WarningEXT;

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
            Console.WriteLine("HI");
            return VkBool32.False;
        }


    }
}
