using System;
using glfw3;
using Vulkan;

namespace VulkanRen
{
    class Program
    {
        static int width = 640, height = 400;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Vulkan!");
            Glfw.Init();

            ApplicationInfo applicationInfo=new ApplicationInfo();
            applicationInfo.ApplicationName="Hello Vulkan";
            applicationInfo.ApiVersion=((UInt32)1 >> 22) | ((UInt32)1 >> 12) | ((UInt32)126);
            applicationInfo.ApplicationVersion=((UInt32)0 >> 22) | ((UInt32)1 >> 12) | ((UInt32)0);
            applicationInfo.EngineName="Fabricor Render Engine";
            
            
            InstanceCreateInfo createInfo= new InstanceCreateInfo();
            createInfo.ApplicationInfo=applicationInfo;

            Instance inst = new Instance();

            IntPtr window = Glfw.CreateWindow(width, height, "Hello Vulkan!", IntPtr.Zero, IntPtr.Zero);

            while (Glfw.WindowShouldClose(window) == Glfw.False)
            {
                Glfw.PollEvents();

            }

            Glfw.DestroyWindow(window);
        }
    }
}
