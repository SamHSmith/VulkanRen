using System;
using System.Numerics;
namespace Fabricor.VulkanRendering
{
    public class FCamera
    {
        public Vector3 position;
        public Quaternion rotation = Quaternion.Identity;

        public float AspectWidth = 1, AspectHeight = 1;
        public float FOV = 90f;

        public float NEAR_PLANE = 0.5f, FAR_PLANE = (float)Math.Pow(10, 10);
        public Matrix4x4 Projection
        {
            get
            {
                return CorrectionProjectionMatrix*Matrix4x4.CreatePerspectiveFieldOfView(FOV / 180 * (float)Math.PI, AspectWidth / AspectHeight, NEAR_PLANE, FAR_PLANE);
            }
        }

        private Matrix4x4 CorrectionProjectionMatrix //Corrects the z axis
        {
            get
            {
                Matrix4x4 matrix=Matrix4x4.Identity;
                matrix.M33=-1;
                return matrix;
            }
        }
        public Matrix4x4 View
        {
            get
            {
                return Matrix4x4.CreateTranslation(-position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(rotation));
            }
        }


    }
}