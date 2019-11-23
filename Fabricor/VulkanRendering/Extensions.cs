using System.Numerics;

namespace Fabricor.VulkanRendering
{
    public static class ExtensionMethods{
        public static float[] ToFloatArray(this Matrix4x4 mat){
            float[] array=new float[16];
            array[0]=mat.M11;
            array[1]=mat.M21;
            array[2]=mat.M31;
            array[3]=mat.M41;

            array[4]=mat.M12;
            array[5]=mat.M22;
            array[6]=mat.M32;
            array[7]=mat.M42;

            array[8]=mat.M13;
            array[9]=mat.M23;
            array[10]=mat.M33;
            array[11]=mat.M43;

            array[12]=mat.M14;
            array[13]=mat.M24;
            array[14]=mat.M34;
            array[15]=mat.M44;
            return array;
        }

    }
}