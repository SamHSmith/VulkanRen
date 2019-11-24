using static Vulkan.VulkanNative;
using Vulkan;

using System.Collections.Concurrent;
using System;

namespace Fabricor.VulkanRendering
{
    public static unsafe class CommandPoolManager
    {
        private static VkDevice device;
        private static ConcurrentDictionary<int,VkCommandPool> pools;

        private static uint graphicsQueueFamily;
        public static void Init(VkDevice device, uint graphicsQueueFamily)
        {
            CommandPoolManager.device = device;
            pools = new ConcurrentDictionary<int,VkCommandPool>();
            CommandPoolManager.graphicsQueueFamily = graphicsQueueFamily;
        }

        public static int CreateCommandPool(VkCommandPoolCreateFlags flags)
        {
            VkCommandPoolCreateInfo createInfo = VkCommandPoolCreateInfo.New();
            createInfo.flags=flags;
            createInfo.queueFamilyIndex=graphicsQueueFamily;

            VkCommandPool pool=VkCommandPool.Null;
            if(vkCreateCommandPool(device,&createInfo,null,&pool)!=VkResult.Success){
                throw new System.Exception("Failed to create commandpool");
            }

            Guid guid=Guid.NewGuid();
            int id=guid.GetHashCode();
            pools[id]=pool;
            return id;
        }

        public static VkCommandPool GetPool(int id){
            return pools[id];
        }
    }
}