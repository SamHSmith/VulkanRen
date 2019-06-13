using System;
using System.Numerics;

namespace Fabricor.Main.Logic
{
    public struct Transform
    {
        public Vector3 position;
        public Quaternion rotation;


        public Transform(Vector3 position)
        {
            this.position = position;
            this.rotation = Quaternion.Identity;
        }

        public Transform(Quaternion rotation)
        {
            this.rotation = rotation;
            this.position = new Vector3();
        }

        public Transform(Vector3 position, Quaternion rotation) : this(position)
        {
            this.rotation = rotation;
        }

        public static Transform operator*(Transform a, Transform b)
        {
            Transform f = new Transform(new Vector3());
            f.position = a.position + Vector3.Transform(b.position, a.rotation);
            f.rotation = Quaternion.Multiply(a.rotation, b.rotation);
            return f;
        }

        public Transform LocalToWorldSpace(Transform t)
        {
            Transform f = new Transform(new Vector3());
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
