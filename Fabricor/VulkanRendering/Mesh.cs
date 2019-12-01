using Vulkan;
using System;
namespace Fabricor.VulkanRendering{
    public class Mesh<T> where T : unmanaged
    {
        public FDataBuffer<T> vertices{get;protected set;}
        public FDataBuffer<uint> indices{get;protected set;}

        public Mesh(VkDevice device,VkPhysicalDevice physicalDevice, T[] vertices,uint[] indices){
            this.vertices=new FDataBuffer<T>(device,physicalDevice,vertices.Length,VkBufferUsageFlags.VertexBuffer,
            VkSharingMode.Exclusive);
            this.indices=new FDataBuffer<uint>(device,physicalDevice,indices.Length,VkBufferUsageFlags.IndexBuffer,VkSharingMode.Exclusive);

            Span<T> spanv=this.vertices.Map();
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

        public void Free(){
            vertices.Free();
            indices.Free();
        }
    }
}