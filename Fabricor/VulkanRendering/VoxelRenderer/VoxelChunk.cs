using System;
namespace Fabricor.VulkanRendering.VoxelRenderer
{
    public struct VoxelRenderChunk
    {

        public const int CHUNK_SIZE = 64;
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
                        bool canBeSeen = IsBlockEmpty(blocks, x + 1, y, z) ||
                        IsBlockEmpty(blocks, x - 1, y, z) ||
                        IsBlockEmpty(blocks, x, y + 1, z) ||
                        IsBlockEmpty(blocks, x, y - 1, z) ||
                        IsBlockEmpty(blocks, x, y, z + 1) ||
                        IsBlockEmpty(blocks, x, y, z - 1);

                        if (canBeSeen)
                            voxels[CoordsToIndex(x, y, z)] = blocks[x, y, z];
                    }
                }
            }
        }
        private bool IsBlockEmpty(ushort[,,] blocks, int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
                return true;

            return blocks[x, y, z] == 0;
        }
        private int CoordsToIndex(int x, int y, int z)
        {
            return (x * CHUNK_SIZE * CHUNK_SIZE) + (y * CHUNK_SIZE) + z;
        }
    }
}