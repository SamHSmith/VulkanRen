using System;
using Vulkan;
using static Vulkan.VulkanNative;
using System.Drawing;

namespace Fabricor.VulkanRendering
{
    public unsafe class FTexture
    {
        public VkImage image;
        public VkImageView imageView;

        public uint MipLevels { get; private set; } = 1;

        public FTexture(VkDevice device, VkPhysicalDevice physicalDevice, int cmdPoolID, VkQueue queue, string[] paths,
        VkFormat format, int width, int height, uint mipLevels = 1)
        {
            MipLevels = mipLevels;
            image = LoadTexture(device, physicalDevice, cmdPoolID, queue, paths, MipLevels);
            imageView = CreateColourImageArrayView(device, image, paths.Length, format, MipLevels);
            InitMips(device, queue, image, width, height, MipLevels, (uint)paths.Length, cmdPoolID);
        }

        private static void InitMips(VkDevice device, VkQueue queue, VkImage image, int width, int height, uint mipLevels, uint layerCount, int cmdPool)
        {
            VkCommandBufferAllocateInfo pAllocateInfo = VkCommandBufferAllocateInfo.New();
            pAllocateInfo.commandPool = CommandPoolManager.GetPool(cmdPool);
            pAllocateInfo.level = VkCommandBufferLevel.Primary;
            pAllocateInfo.commandBufferCount = 1;

            VkCommandBuffer cmdBuffer = VkCommandBuffer.Null;
            Assert(vkAllocateCommandBuffers(device, &pAllocateInfo, &cmdBuffer));


            Assert(vkResetCommandBuffer(cmdBuffer, VkCommandBufferResetFlags.None));
            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

            Assert(vkBeginCommandBuffer(cmdBuffer, &beginInfo));

            VkImageMemoryBarrier imageMemoryBarrier = VkImageMemoryBarrier.New();
            imageMemoryBarrier.srcAccessMask = VkAccessFlags.None;
            imageMemoryBarrier.dstAccessMask = VkAccessFlags.TransferWrite;
            imageMemoryBarrier.oldLayout = VkImageLayout.Undefined;
            imageMemoryBarrier.newLayout = VkImageLayout.TransferDstOptimal;
            imageMemoryBarrier.srcQueueFamilyIndex = VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.dstQueueFamilyIndex = VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.image = image;
            imageMemoryBarrier.subresourceRange = new VkImageSubresourceRange()
            {
                baseMipLevel = 0,
                levelCount = mipLevels,
                baseArrayLayer = 0,
                layerCount = layerCount,
                aspectMask = VkImageAspectFlags.Color
            };

            vkCmdPipelineBarrier(cmdBuffer, VkPipelineStageFlags.AllCommands, VkPipelineStageFlags.AllCommands, VkDependencyFlags.ByRegion,
            0, null, 0, null, 1, &imageMemoryBarrier);

            for (int i = 1; i < mipLevels; i++)
            {
                imageMemoryBarrier.oldLayout = VkImageLayout.TransferDstOptimal;
                imageMemoryBarrier.newLayout = VkImageLayout.TransferSrcOptimal;
                imageMemoryBarrier.srcAccessMask = VkAccessFlags.TransferWrite;
                imageMemoryBarrier.dstAccessMask = VkAccessFlags.TransferRead;
                imageMemoryBarrier.subresourceRange.baseMipLevel = (uint)(i - 1);
                imageMemoryBarrier.subresourceRange.levelCount = 1;
                vkCmdPipelineBarrier(cmdBuffer, VkPipelineStageFlags.AllCommands, VkPipelineStageFlags.AllCommands, VkDependencyFlags.ByRegion,
            0, null, 0, null, 1, &imageMemoryBarrier);

                VkImageBlit blit = new VkImageBlit();
                blit.srcOffsets_0 = new VkOffset3D { x = 0, y = 0, z = 0 };
                blit.srcOffsets_1 = new VkOffset3D { x = width, y = height, z = 1 };
                blit.srcSubresource.aspectMask = VkImageAspectFlags.Color;
                blit.srcSubresource.mipLevel = (uint)(i - 1);
                blit.srcSubresource.baseArrayLayer = 0;
                blit.srcSubresource.layerCount = layerCount;
                blit.dstOffsets_0 = new VkOffset3D { x = 0, y = 0, z = 0 };
                blit.dstOffsets_1 = new VkOffset3D { x = width / 2, y = height / 2, z = 1 };
                blit.dstSubresource.aspectMask = VkImageAspectFlags.Color;
                blit.dstSubresource.mipLevel = (uint)i;
                blit.dstSubresource.baseArrayLayer = 0;
                blit.dstSubresource.layerCount = layerCount;
                vkCmdBlitImage(cmdBuffer, image, VkImageLayout.TransferSrcOptimal, image, VkImageLayout.TransferDstOptimal,
                1, &blit, VkFilter.Linear);
                width /= 2;
                height /= 2;

                imageMemoryBarrier.oldLayout = VkImageLayout.TransferSrcOptimal;
                imageMemoryBarrier.newLayout = VkImageLayout.ShaderReadOnlyOptimal;
                imageMemoryBarrier.srcAccessMask = VkAccessFlags.TransferRead;
                imageMemoryBarrier.dstAccessMask = VkAccessFlags.ShaderRead;
                imageMemoryBarrier.subresourceRange.baseMipLevel = (uint)(i - 1);
                imageMemoryBarrier.subresourceRange.levelCount = 1;
                vkCmdPipelineBarrier(cmdBuffer, VkPipelineStageFlags.AllCommands, VkPipelineStageFlags.AllCommands, VkDependencyFlags.ByRegion,
            0, null, 0, null, 1, &imageMemoryBarrier);
            }

            imageMemoryBarrier.oldLayout = VkImageLayout.Undefined;
            imageMemoryBarrier.newLayout = VkImageLayout.ShaderReadOnlyOptimal;
            imageMemoryBarrier.srcAccessMask = VkAccessFlags.TransferRead;
            imageMemoryBarrier.dstAccessMask = VkAccessFlags.ShaderRead;
            imageMemoryBarrier.subresourceRange.baseMipLevel = mipLevels - 1;
            imageMemoryBarrier.subresourceRange.levelCount = 1;
            vkCmdPipelineBarrier(cmdBuffer, VkPipelineStageFlags.AllCommands, VkPipelineStageFlags.AllCommands, VkDependencyFlags.ByRegion,
            0, null, 0, null, 1, &imageMemoryBarrier);

            Assert(vkEndCommandBuffer(cmdBuffer));

            VkSubmitInfo submitInfo = VkSubmitInfo.New();
            submitInfo.commandBufferCount = 1;
            submitInfo.pCommandBuffers = &cmdBuffer;
            Assert(vkQueueSubmit(queue, 1, &submitInfo, VkFence.Null));
        }
        public static VkImageView CreateColourImageView(VkDevice device, VkImage image, VkFormat imageFormat)
        {
            VkImageViewCreateInfo createInfo = VkImageViewCreateInfo.New();
            createInfo.image = image;
            createInfo.viewType = VkImageViewType.Image2D;
            createInfo.format = imageFormat;
            createInfo.subresourceRange.aspectMask = VkImageAspectFlags.Color;
            createInfo.subresourceRange.layerCount = 1;
            createInfo.subresourceRange.levelCount = 1;

            VkImageView view = VkImageView.Null;
            Assert(vkCreateImageView(device, &createInfo, null, &view));
            return view;
        }

        public static VkImageView CreateColourImageArrayView(VkDevice device, VkImage image, int layers, VkFormat imageFormat, uint mipLevels)
        {
            VkImageViewCreateInfo createInfo = VkImageViewCreateInfo.New();
            createInfo.image = image;
            createInfo.viewType = VkImageViewType.Image2DArray;
            createInfo.format = imageFormat;
            createInfo.subresourceRange.aspectMask = VkImageAspectFlags.Color;
            createInfo.subresourceRange.layerCount = (uint)layers;
            createInfo.subresourceRange.levelCount = mipLevels;

            VkImageView view = VkImageView.Null;
            Assert(vkCreateImageView(device, &createInfo, null, &view));
            return view;
        }

        private static VkImage LoadTexture(VkDevice device, VkPhysicalDevice physicalDevice, int cmdPoolID, VkQueue queue, string[] paths, uint mipLevels)
        {
            Bitmap[] bitmaps = new Bitmap[paths.Length];
            FDataBuffer<byte>[] tempBuffer = new FDataBuffer<byte>[paths.Length];

            uint width = 0, height = 0;

            for (int j = 0; j < paths.Length; j++)
            {
                bitmaps[j] = new Bitmap(System.Drawing.Image.FromFile(paths[j]));

                var data = bitmaps[j].LockBits(new Rectangle(0, 0, bitmaps[j].Width, bitmaps[j].Height), System.Drawing.Imaging.ImageLockMode.ReadOnly,
                bitmaps[j].PixelFormat);
                width = (uint)data.Width;
                height = (uint)data.Height;//TODO add size check


                Span<byte> img = new Span<byte>((void*)data.Scan0, data.Stride * data.Height);

                tempBuffer[j] = new FDataBuffer<byte>(device, physicalDevice, img.Length, VkBufferUsageFlags.TransferSrc,
                VkSharingMode.Exclusive);

                Span<byte> buffer = tempBuffer[j].Map();
                for (int i = 0; i < img.Length; i += 4)
                {

                    buffer[i + 2] = img[i];
                    buffer[i + 1] = img[i + 1];
                    buffer[i] = img[i + 2];
                    buffer[i + 3] = img[i + 3];
                }
                buffer = tempBuffer[j].UnMap();
            }

            VkImage texture = VkImage.Null;
            VkDeviceMemory memory = VkDeviceMemory.Null;

            VkImageCreateInfo createInfo = VkImageCreateInfo.New();
            createInfo.imageType = VkImageType.Image2D;
            createInfo.extent.width = width;
            createInfo.extent.height = height;
            createInfo.extent.depth = 1;
            createInfo.mipLevels = mipLevels;
            createInfo.arrayLayers = (uint)paths.Length;
            createInfo.format = VkFormat.R8g8b8a8Unorm;
            createInfo.tiling = VkImageTiling.Optimal;
            createInfo.initialLayout = VkImageLayout.Undefined;
            createInfo.usage = VkImageUsageFlags.TransferDst | VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferSrc;
            createInfo.sharingMode = VkSharingMode.Exclusive;
            createInfo.samples = VkSampleCountFlags.Count1;

            Assert(vkCreateImage(device, &createInfo, null, &texture));

            VkMemoryRequirements memoryRequirements;
            vkGetImageMemoryRequirements(device, texture, &memoryRequirements);

            VkPhysicalDeviceMemoryProperties memoryProperties = new VkPhysicalDeviceMemoryProperties();
            vkGetPhysicalDeviceMemoryProperties(physicalDevice, &memoryProperties);

            VkMemoryAllocateInfo allocateInfo = VkMemoryAllocateInfo.New();
            allocateInfo.allocationSize = memoryRequirements.size;
            allocateInfo.memoryTypeIndex = FDataBuffer<byte>.SelectMemoryType(memoryProperties, memoryRequirements.memoryTypeBits,
            VkMemoryPropertyFlags.DeviceLocal);

            Assert(vkAllocateMemory(device, &allocateInfo, null, &memory));

            vkBindImageMemory(device, texture, memory, 0);

            VkCommandBufferAllocateInfo pAllocateInfo = VkCommandBufferAllocateInfo.New();
            pAllocateInfo.commandPool = CommandPoolManager.GetPool(cmdPoolID);
            pAllocateInfo.level = VkCommandBufferLevel.Primary;
            pAllocateInfo.commandBufferCount = 1;

            VkCommandBuffer cmdBuffer = VkCommandBuffer.Null;
            Assert(vkAllocateCommandBuffers(device, &pAllocateInfo, &cmdBuffer));

            VkCommandBufferBeginInfo beginInfo = VkCommandBufferBeginInfo.New();
            beginInfo.flags = VkCommandBufferUsageFlags.OneTimeSubmit;

            Assert(vkBeginCommandBuffer(cmdBuffer, &beginInfo));

            VkImageMemoryBarrier imageMemoryBarrier = VkImageMemoryBarrier.New();
            imageMemoryBarrier.srcAccessMask = VkAccessFlags.None;
            imageMemoryBarrier.dstAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite;
            imageMemoryBarrier.oldLayout = VkImageLayout.Undefined;
            imageMemoryBarrier.newLayout = VkImageLayout.TransferDstOptimal;
            imageMemoryBarrier.srcQueueFamilyIndex = VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.dstQueueFamilyIndex = VulkanNative.QueueFamilyIgnored;
            imageMemoryBarrier.image = texture;
            imageMemoryBarrier.subresourceRange = new VkImageSubresourceRange()
            {
                baseMipLevel = 0,
                levelCount = mipLevels,
                baseArrayLayer = 0,
                layerCount = (uint)paths.Length,
                aspectMask = VkImageAspectFlags.Color
            };

            vkCmdPipelineBarrier(cmdBuffer, VkPipelineStageFlags.AllCommands, VkPipelineStageFlags.AllCommands, VkDependencyFlags.ByRegion,
            0, null, 0, null, 1, &imageMemoryBarrier);

            for (int j = 0; j < tempBuffer.Length; j++)
            {
                VkBufferImageCopy region = new VkBufferImageCopy();
                region.bufferOffset = 0;
                region.bufferRowLength = 0;
                region.bufferImageHeight = 0;

                region.imageSubresource.aspectMask = VkImageAspectFlags.Color;
                region.imageSubresource.mipLevel = 0;
                region.imageSubresource.baseArrayLayer = (uint)j;
                region.imageSubresource.layerCount = 1;

                region.imageOffset = new VkOffset3D();
                region.imageExtent = new VkExtent3D() { width = width, height = height, depth = 1 };

                vkCmdCopyBufferToImage(cmdBuffer, tempBuffer[j].Buffer, texture, VkImageLayout.TransferDstOptimal, 1, &region);
            }

            imageMemoryBarrier.oldLayout = VkImageLayout.TransferDstOptimal;
            imageMemoryBarrier.newLayout = VkImageLayout.ShaderReadOnlyOptimal;

            vkCmdPipelineBarrier(cmdBuffer, VkPipelineStageFlags.AllCommands, VkPipelineStageFlags.AllCommands, VkDependencyFlags.ByRegion,
            0, null, 0, null, 1, &imageMemoryBarrier);

            Assert(vkEndCommandBuffer(cmdBuffer));

            VkSubmitInfo submitInfo = VkSubmitInfo.New();
            submitInfo.commandBufferCount = 1;
            submitInfo.pCommandBuffers = &cmdBuffer;

            Assert(vkQueueSubmit(queue, 1, &submitInfo, VkFence.Null));
            Assert(vkQueueWaitIdle(queue));
            vkFreeCommandBuffers(device, CommandPoolManager.GetPool(cmdPoolID), 1, &cmdBuffer);

            return texture;
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