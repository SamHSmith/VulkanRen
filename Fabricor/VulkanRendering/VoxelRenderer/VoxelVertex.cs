using System.Numerics;
namespace Fabricor.VulkanRendering.VoxelRenderer
{
    public struct VoxelVertex{
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texcoords;
        public uint textureId;
    }
}