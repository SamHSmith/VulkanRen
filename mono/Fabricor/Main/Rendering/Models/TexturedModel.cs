using System;
using Fabricor.Main.Rendering.textures;

namespace Fabricor.Main.Rendering.models
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
