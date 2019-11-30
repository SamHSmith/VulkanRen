using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Fabricor.VulkanRendering.VoxelRenderer
{
    public class VoxelMesh{
        public FDataBuffer<VoxelVertex> vertices;
        public FDataBuffer<uint> indices;

        public VoxelMesh(VkDevice device,VkPhysicalDevice physicalDevice, VoxelVertex[] vertices,uint[] indices){
            this.vertices=new FDataBuffer<VoxelVertex>(device,physicalDevice,vertices.Length,VkBufferUsageFlags.VertexBuffer,
            VkSharingMode.Exclusive);
            this.indices=new FDataBuffer<uint>(device,physicalDevice,indices.Length,VkBufferUsageFlags.IndexBuffer,VkSharingMode.Exclusive);

            Span<VoxelVertex> spanv=this.vertices.Map();
            for (int i = 0; i < vertices.Length; i++)
            {
                spanv[i]=vertices[i];
            }
            spanv=this.vertices.UnMap();

            Span<uint> spani=this.indices.Map();
            for (int i = 0; i < indices.Length; i++)
            {
                spani[i]=indices[i];
            }
            spani=this.indices.UnMap();
        }
    }
}