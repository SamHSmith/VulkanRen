using System;
using System.Numerics;

namespace Fabricor.Main.Logic
{
    public class Transform
    {
        public Vector3 position=new Vector3();
        public Quaternion rotation=Quaternion.Identity;

        public Transform LocalToWorldSpace(Transform t)
        {
            Transform f = new Transform();
            f.position = position + Vector3.Transform(t.position, rotation);
            f.rotation = Quaternion.Multiply(rotation, t.rotation);
            return f;
        }

        public OpenTK.Matrix4 ToGLMatrix()
        {
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateTranslation(position.X, position.Y, position.Z);
            mat = OpenTK.Matrix4.Mult(OpenTK.Matrix4.CreateFromQuaternion(new OpenTK.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W)),mat);
            return mat;
        }
    }
}
