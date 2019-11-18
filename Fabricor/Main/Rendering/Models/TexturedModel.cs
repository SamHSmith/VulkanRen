using System;
using Fabricor.Main.Rendering.Textures;

namespace Fabricor.Main.Rendering.Models
{
    public class TexturedModel
    {
        public RawModel RawModel { get; private set; }
        public ModelTexture Texture { get; private set; }

        public TexturedModel(RawModel rawModel, ModelTexture texture)
        {
            RawModel = rawModel;
            Texture = texture;
        }
    }
}
