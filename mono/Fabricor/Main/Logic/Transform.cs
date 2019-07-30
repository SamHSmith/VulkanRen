using System;
using System.Numerics;
using BepuPhysics;

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

<<<<<<< HEAD
        public static Transform operator*(Transform a, Transform b)
        {
            Transform f = new Transform(new Vector3());
            f.position = a.position + Vector3.Transform(b.position, a.rotation);
            f.rotation = Quaternion.Multiply(a.rotation, b.rotation);
            return f;
        }

        public static Transform operator /(Transform b, Transform a)
=======
        public RigidPose ToRigidPose()
        {
            BepuUtilities.Quaternion rot = new BepuUtilities.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
            RigidPose pose = new RigidPose(in position, in rot);
            return pose;
        }

        public void FromRigidPose(RigidPose p)
        {
            rotation = new Quaternion(p.Orientation.X, p.Orientation.Y, p.Orientation.Z, p.Orientation.W);
            position = p.Position;
        }

        public Transform LocalToWorldSpace(Transform t)
>>>>>>> c309e982ebb6a38a7f1240baf92ec09938fe4f60
        {
            Transform f = new Transform(new Vector3());
            f.position = Vector3.Transform(b.position- a.position, a.rotation);
            f.rotation = Quaternion.Multiply(Quaternion.Inverse(a.rotation), b.rotation);
            return f;
        }

        public Transform LocalToWorldSpace(Transform t)
        {

            return this*t;
        }

        public OpenTK.Matrix4 ToGLMatrix()
        {
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateTranslation(position.X, position.Y, position.Z);
            mat = OpenTK.Matrix4.Mult(OpenTK.Matrix4.CreateFromQuaternion(new OpenTK.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W)),mat);
            return mat;
        }
    }
}
