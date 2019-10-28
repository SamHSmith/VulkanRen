using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GLFW;
using Vulkan;

namespace VulkanRen
{
    unsafe class Program
    {
        static int width = 640, height = 400;
        static VkDebugReportCallbackEXT debugReport;
        static void Assert(VkResult result)
        {
            if (result != VkResult.Success)
            {
                Console.Error.WriteLine("Vulkan Error: " + result);
                throw new System.Exception("Vulkan Error: " + result);
            }
        }
        static VkInstance CreateInstance()
        {
            VkApplicationInfo applicationInfo = new VkApplicationInfo();
            applicationInfo.apiVersion = new Vulkan.Version(1, 1, 0);

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
            IntPtr inst = (IntPtr)(&instance);
            fixed (byte** layers = &ppDebugLayerArray[0])
            {

                fixed (byte** extensions = &ppExtensionNamesArray[0])
                {
                    VkInstanceCreateInfo createInfo = new VkInstanceCreateInfo();
                    createInfo.pApplicationInfo = (IntPtr)(&applicationInfo);
                    createInfo.ppEnabledLayerNames = (IntPtr)layers;
                    createInfo.enabledLayerCount = (uint)pDebugLayers.Length;
                    createInfo.ppEnabledExtensionNames = (IntPtr)extensions;

                    createInfo.enabledExtensionCount = (uint)extensionNames.Length;
                    Assert(Vk.vkCreateInstance((IntPtr)(&createInfo), IntPtr.Zero, inst));
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
            createInfoEXT.flags = VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.ErrorEXT;


            byte[] debugExtFnName = Encoding.UTF8.GetBytes("vkCreateDebugReportCallbackEXT" + char.MinValue);



            IntPtr createFnPtr;

            fixed (byte* namePtr = &(debugExtFnName[0]))
            {
                createFnPtr = Vk.vkGetInstanceProcAddr(instance, (IntPtr)namePtr);
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
        private static VkBool32 DebugCallback(VkDebugReportFlagsEXT flags, VkDebugReportObjectTypeEXT objectType,
        ulong @object, UIntPtr location, int messageCode, IntPtr pLayerPrefix, IntPtr pMessage, IntPtr pUserData)
        {
            Console.WriteLine(*(byte*)pMessage);
            return VkBool32.False;
        }

        static VkPhysicalDevice _PickPhysicsDevice(VkPhysicalDevice[] physicalDevices, uint deviceCount)
        {
            for (int i = 0; i < deviceCount; i++)
            {
                VkPhysicalDeviceProperties props = new VkPhysicalDeviceProperties();
                Vk.vkGetPhysicalDeviceProperties(physicalDevices[i], (IntPtr)(&props));

                if (props.deviceType == VkPhysicalDeviceType.DiscreteGpu)
                {

                    Console.WriteLine("Picking discrete GPU: " + Marshal.PtrToStringUTF8((IntPtr)props.deviceName));
                    return physicalDevices[i];
                }
            }

            return physicalDevices[0];
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Vulkan!");
            Glfw.Init();


            VkInstance inst = CreateInstance();
            
            uint deviceCount = 0;
            Assert(Vk.vkEnumeratePhysicalDevices(inst, (IntPtr)(&deviceCount), IntPtr.Zero));

            VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[deviceCount];

            fixed (VkPhysicalDevice* devPtr = &physicalDevices[0])
            {
                Assert(Vk.vkEnumeratePhysicalDevices(inst, (IntPtr)(&deviceCount), (IntPtr)devPtr));
            }

            VkPhysicalDevice physicalDevice = _PickPhysicsDevice(physicalDevices, deviceCount);

            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);
            Window window = Glfw.CreateWindow(width, height, "Hello Vulkan!", Monitor.None, Window.None);
            Console.WriteLine("HI");
            while (!Glfw.WindowShouldClose(window))
            {
                Glfw.PollEvents();

            }

            Glfw.DestroyWindow(window);
        }
        /*
                static Bool32 DebugReportCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
                {
                    string layerString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(layerPrefix);
                    string messageString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);

                    Console.WriteLine("DebugReport layer: {0} message: {1}", layerString, messageString);

                    return false;
                }*/
    }
}
