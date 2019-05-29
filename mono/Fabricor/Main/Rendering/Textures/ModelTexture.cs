using System;
namespace Fabricor.Main.Rendering.Textures
{
    public class ModelTexture
    {
        private int _textureID;
        public int textureID { get { return _textureID; } set { _textureID = value; } }

        public ModelTexture(int textureID)
        {
            _textureID = textureID;
        }
    }
}
