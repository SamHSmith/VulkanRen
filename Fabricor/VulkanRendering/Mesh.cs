using System.Threading.Tasks;
using Vulkan;
using System;
namespace Fabricor.VulkanRendering
{
    public class Mesh<T> where T : unmanaged
    {
        public FDataBuffer<T> vertices { get; protected set; }
        public FDataBuffer<uint> indices { get; protected set; }

        public unsafe Mesh(VkDevice device, VkPhysicalDevice physicalDevice, T[] vertices, uint[] indices)
        {
            this.vertices = new FDataBuffer<T>(device, physicalDevice, vertices.Length, VkBufferUsageFlags.VertexBuffer,
            VkSharingMode.Exclusive);
            this.indices = new FDataBuffer<uint>(device, physicalDevice, indices.Length, VkBufferUsageFlags.IndexBuffer, VkSharingMode.Exclusive);

            Span<T> spanv = this.vertices.Map();
            T* vptr;
            fixed (T* ptr = spanv)
                vptr = ptr;
            Parallel.For(0, vertices.Length, (i) =>
            {
                *(vptr + i) = vertices[i];
            });
            vptr=null;
            spanv = this.vertices.UnMap();

            Span<uint> spani = this.indices.Map();
            uint* iptr;
            fixed(uint* ptr=spani)
                iptr=ptr;
            Parallel.For(0, indices.Length, (i) =>
            {
                *(iptr + i) = indices[i];
            });
            iptr=null;
            spani = this.indices.UnMap();
        }

        public void Free()
        {
            vertices.Free();
            indices.Free();
        }
    }
}