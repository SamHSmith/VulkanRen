using System;
namespace Fabricor.VulkanRendering.VoxelRenderer
{
    public struct VoxelRenderChunk
    {

        public const int CHUNK_SIZE = 16;
        public ushort[] voxels { get; private set; }

        public Span<ushort> Span
        {
            get
            {
                return voxels.AsSpan();
            }
        }

        public VoxelRenderChunk(ushort[,,] blocks)
        {
            voxels = new ushort[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        voxels[CoordsToIndex(x, y, z)] = blocks[x, y, z];
                    }
                }
            }
        }
        private int CoordsToIndex(int x, int y, int z)
        {
            return (x * CHUNK_SIZE * CHUNK_SIZE) + (y * CHUNK_SIZE) + z;
        }
    }
}