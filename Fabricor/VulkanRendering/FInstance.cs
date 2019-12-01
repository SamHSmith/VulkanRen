using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System;

using static Vulkan.VulkanNative;
using Vulkan;

namespace Fabricor.VulkanRendering{
    unsafe class FInstance{

        static void Assert(VkResult result)
        {
            if (result != VkResult.Success)
            {
                Console.Error.WriteLine($"Error: {result}");
                throw new System.Exception($"Error: {result}");
            }
        }

        
        
        public VkInstance instance;

        public FInstance(){
            this.instance=CreateInstance();
        }

        public void Destroy(){
            vkDestroyInstance(instance,null);
        }

        private VkInstance CreateInstance()
        {
            List<GCHandle> handles = new List<GCHandle>();

            bool debug = true;

            string[] debugOnlyLayers = new string[] {
                //"VK_LAYER_GOOGLE_threading",
                //"VK_LAYER_LUNARG_parameter_validation",
                //"VK_LAYER_LUNARG_device_limits", Not Present?
                //"VK_LAYER_LUNARG_object_tracker",
                //"VK_LAYER_LUNARG_image", Not Present?
                "VK_LAYER_LUNARG_core_validation",
                //"VK_LAYER_LUNARG_swapchain",
                //"VK_LAYER_GOOGLE_unique_objects",
            };
            List<string> layerList = new List<string>();
            if (debug)
                layerList.AddRange(debugOnlyLayers);

            string[] layers = layerList.ToArray();
            byte[][] pDebugLayers = new byte[layers.Length][];


            byte*[] ppDebugLayerArray = new byte*[pDebugLayers.Length];
            if (!debug)
                ppDebugLayerArray = new byte*[1];//this is to give a null ptr to use later


            for (int i = 0; i < pDebugLayers.Length; i++)
            {
                pDebugLayers[i] = Encoding.UTF8.GetBytes(layers[i] + char.MinValue);
                GCHandle handle = GCHandle.Alloc(pDebugLayers[i]);
                handles.Add(handle);
                fixed (byte* p = &(((byte[])handle.Target)[0]))
                {
                    ppDebugLayerArray[i] = p;
                }
            }

            List<string> requiredExtensions = new List<string>();
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
            fixed (byte** layersptr = &ppDebugLayerArray[0])
            {

                fixed (byte** extensions = &ppExtensionNamesArray[0])
                {
                    VkInstanceCreateInfo pCreateInfo = VkInstanceCreateInfo.New();
                    pCreateInfo.ppEnabledLayerNames = layersptr;
                    pCreateInfo.ppEnabledExtensionNames = extensions;
                    pCreateInfo.enabledLayerCount = (uint)pDebugLayers.Length;
                    pCreateInfo.enabledExtensionCount = (uint)extensionNames.Length;

                    Assert(vkCreateInstance(&pCreateInfo, null, &instance));

                }
            }

            foreach (var handle in handles)
            {
                handle.Free();
            }

            DebugDelegate debugDelegate=new DebugDelegate(DebugCallback);

            //PFN_vkDebugReportCallbackEXT _debugCallbackFunc =(PFN_vkDebugReportCallbackEXT) debugDelegate;

            

            IntPtr debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(debugDelegate);

            debugDelegateHandle=GCHandle.Alloc(debugDelegate);

            VkDebugReportCallbackCreateInfoEXT createInfoEXT = VkDebugReportCallbackCreateInfoEXT.New();
            createInfoEXT.pfnCallback = debugFunctionPtr;
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

        static VkDebugReportCallbackEXT debugReport = new VkDebugReportCallbackEXT();

        public delegate uint DebugDelegate(uint flags, VkDebugReportObjectTypeEXT objectType, ulong @object, UIntPtr location,
         int messageCode, byte* pLayerPrefix, byte* pMessage, void* pUserData);
        

        static GCHandle debugDelegateHandle;
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