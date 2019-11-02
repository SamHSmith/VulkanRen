using static Vulkan.VulkanNative;
using Vulkan;
using System;

namespace Fabricor.Vulkan
{
    public unsafe class FCommandBuffer{

        public VkCommandBuffer buffer;
        public VkRenderPass renderPass;
        public VkFramebuffer framebuffer;
        public VkPipeline pipeline;

        public FCommandBuffer(VkDevice device, VkCommandPool pool){
            VkCommandBufferAllocateInfo pAllocateInfo = VkCommandBufferAllocateInfo.New();
            pAllocateInfo.commandPool = pool;
            pAllocateInfo.level = VkCommandBufferLevel.Primary;
            pAllocateInfo.commandBufferCount = 1;

            VkCommandBuffer cmdBuffer = VkCommandBuffer.Null;
            Assert(vkAllocateCommandBuffers(device, &pAllocateInfo, &cmdBuffer));
            
            buffer= cmdBuffer;
        }

        public void RecordCommandBuffer(){
                VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

                Assert(vkBeginCommandBuffer(buffer, &beginInfo));

                VkClearColorValue clearColorValue = new VkClearColorValue { float32_0 = 0, float32_1 = 1f / 10, float32_2 = 1f / 10, float32_3 = 1 };
                VkClearValue clearValue = new VkClearValue();
                clearValue.color = clearColorValue;

                VkRenderPassBeginInfo passBeginInfo = VkRenderPassBeginInfo.New();
                passBeginInfo.renderPass = renderPass;
                passBeginInfo.framebuffer = framebuffer;
                passBeginInfo.renderArea.extent.width = (uint)Program.width;
                passBeginInfo.renderArea.extent.height = (uint)Program.height;
                passBeginInfo.clearValueCount = 1;
                passBeginInfo.pClearValues = &clearValue;

                vkCmdBeginRenderPass(buffer, &passBeginInfo, VkSubpassContents.Inline);

                VkViewport viewport = new VkViewport();
                viewport.x = 0;
                viewport.y = (float)Program.height;
                viewport.width = (float)Program.width;
                viewport.height = -(float)Program.height;

                VkRect2D scissor = new VkRect2D();
                scissor.offset.x = 0;
                scissor.offset.y = 0;
                scissor.extent.width = (uint)Program.width;
                scissor.extent.height = (uint)Program.height;

                vkCmdSetViewport(buffer, 0, 1, &viewport);
                vkCmdSetScissor(buffer, 0, 1, &scissor);

                vkCmdBindPipeline(buffer, VkPipelineBindPoint.Graphics, pipeline);
                vkCmdDraw(buffer, 3, 1, 0, 0);

                vkCmdEndRenderPass(buffer);

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