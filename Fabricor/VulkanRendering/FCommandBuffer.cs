using static Vulkan.VulkanNative;
using Vulkan;
using System;

namespace Fabricor.VulkanRendering
{
    public unsafe class FCommandBuffer
    {

        public VkCommandBuffer buffer;
        public VkRenderPass renderPass;
        public VkFramebuffer framebuffer;
        public VkPipeline pipeline;
        public VkImage image;
        public VkDescriptorSet descriptorSet;
        public VkPipelineLayout layout;


        public FDataBuffer<VoxelRenderer.VoxelVertex> dataBuffer;

        public FCommandBuffer(VkDevice device, VkCommandPool pool)
        {
            VkCommandBufferAllocateInfo pAllocateInfo = VkCommandBufferAllocateInfo.New();
            pAllocateInfo.commandPool = pool;
            pAllocateInfo.level = VkCommandBufferLevel.Primary;
            pAllocateInfo.commandBufferCount = 1;

            VkCommandBuffer cmdBuffer = VkCommandBuffer.Null;
            Assert(vkAllocateCommandBuffers(device, &pAllocateInfo, &cmdBuffer));

            buffer = cmdBuffer;
        }

        public void RecordCommandBuffer()
        {
            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

            Assert(vkBeginCommandBuffer(buffer, &beginInfo));

            VkImageMemoryBarrier imageMemoryBarrier=VkImageMemoryBarrier.New();
            imageMemoryBarrier.srcAccessMask=VkAccessFlags.None;
            imageMemoryBarrier.dstAccessMask=VkAccessFlags.ColorAttachmentRead|VkAccessFlags.ColorAttachmentWrite;
            imageMemoryBarrier.oldLayout=VkImageLayout.Undefined;
            imageMemoryBarrier.newLayout=VkImageLayout.ColorAttachmentOptimal;
            imageMemoryBarrier.srcQueueFamilyIndex=VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.dstQueueFamilyIndex=VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.image=image;
            imageMemoryBarrier.subresourceRange=new VkImageSubresourceRange(){baseMipLevel=0,levelCount=1,
            baseArrayLayer=0,layerCount=1,aspectMask=VkImageAspectFlags.Color};

            vkCmdPipelineBarrier(buffer,VkPipelineStageFlags.AllGraphics,VkPipelineStageFlags.AllGraphics,VkDependencyFlags.ByRegion,
            0,null,0,null,1,&imageMemoryBarrier);

            VkClearColorValue clearColorValue = new VkClearColorValue { float32_0 = 161/255f, float32_1 = 96f / 255, float32_2 = 39f / 255, float32_3 = 1 };
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

            VkBuffer[] databuffers = new VkBuffer[] { dataBuffer.Buffer,dataBuffer.Buffer,dataBuffer.Buffer,dataBuffer.Buffer };
            ulong[] offsets = new ulong[] { 0,3*4,6*4,6*4+2*4 };
            fixed (VkBuffer* bptr = databuffers)
                fixed (ulong* optr = offsets)
                    vkCmdBindVertexBuffers(buffer, 0, 4, bptr, optr);
            
            VkDescriptorSet sets=descriptorSet;
            VkPipelineLayout layout=this.layout;

            vkCmdBindDescriptorSets(buffer,VkPipelineBindPoint.Graphics,layout,0,1,&sets,0,null);
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