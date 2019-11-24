using static Vulkan.VulkanNative;
using Vulkan;
using System;

namespace Fabricor.VulkanRendering
{
    public unsafe class FCommandBuffer
    {

        public VkCommandBuffer buffer;


        public FDataBuffer<VoxelRenderer.VoxelVertex> dataBuffer;

        public FCommandBuffer(VkDevice device, int pool)
        {
            VkCommandBufferAllocateInfo pAllocateInfo = VkCommandBufferAllocateInfo.New();
            pAllocateInfo.commandPool = CommandPoolManager.GetPool(pool);
            pAllocateInfo.level = VkCommandBufferLevel.Primary;
            pAllocateInfo.commandBufferCount = 1;

            VkCommandBuffer cmdBuffer = VkCommandBuffer.Null;
            Assert(vkAllocateCommandBuffers(device, &pAllocateInfo, &cmdBuffer));

            buffer = cmdBuffer;
        }

        public void RecordCommandBuffer(Action<VkCommandBuffer>[] executions)
        {
            Assert(vkResetCommandBuffer(buffer,VkCommandBufferResetFlags.None));
            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

            Assert(vkBeginCommandBuffer(buffer, &beginInfo));

            for (int i = 0; i < executions.Length; i++)
            {
                executions[i](buffer);
            }

            Assert(vkEndCommandBuffer(buffer));
        }


        static void Assert(VkResult result)
        {
            if (result != VkResult.Success)
            {
                Console.Error.WriteLine($"Error: {result}");
                throw new System.Exception($"Error: {result}");
            }
        }
    }
}