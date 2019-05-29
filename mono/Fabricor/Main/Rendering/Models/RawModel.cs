using System;
namespace Fabricor.Main.Rendering.models
{
    public class RawModel
    {
        private int _vaoID;
        public int vaoID { get { return _vaoID; } set { _vaoID = value; } }
        private int _vertexCount;
        public int vertexCount { get { return _vertexCount; } set { _vertexCount = value; } }


        public RawModel(int vaoID, int vertexCount)
        {
            this._vaoID = vaoID;
            this._vertexCount = vertexCount;
        }

        
    }
}
