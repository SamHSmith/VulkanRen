using System;
using Fabricor.Main.Rendering;
using Fabricor.Main.Rendering.Loading;
using Fabricor.Main.Rendering.Textures;

namespace Fabricor.Main.Logic.Grids
{
    public class BlockLookup
    {
        public const string TextureAtlasName = "BlockTest";

        private static Mesh blockMesh = OBJLoader.LoadFromOBJ("Block");
        public static ModelTexture AtlasTexture { get; private set; } = new ModelTexture(MasterRenderer.GlLoader.LoadTexture(TextureAtlasName));

        public static Mesh GetBlockMesh(ushort block)
        {
            return blockMesh;
        }
    }
}
