using System;
namespace Fabricor.Main.Rendering.Models
{
    public class DynamicModel : RawModel
    {
        public int[] vbos;

        public DynamicModel(int vaoID, int vertexCount,int[] vbos) : base(vaoID, vertexCount)
        {
            this.vbos = vbos;
        }
    }
}
