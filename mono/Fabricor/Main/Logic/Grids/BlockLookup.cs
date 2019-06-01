using System;
using Fabricor.Main.Rendering.Loading;

namespace Fabricor.Main.Logic.Grids
{
    public class BlockLookup
    {
        private static Mesh blockMesh = OBJLoader.LoadFromOBJ("Block");

        public static Mesh GetBlockMesh(uint block)
        {
            return blockMesh;
        }
    }
}
