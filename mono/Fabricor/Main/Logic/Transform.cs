using System;
using System.Numerics;
using BepuPhysics;

namespace Fabricor.Main.Logic
{
    public class Transform
    {
        public Vector3 position=new Vector3();
        public Quaternion rotation=Quaternion.Identity;

        public Transform()
        {
        }

        public Transform(Vector3 position)
        {
            this.position = position;
        }

        public Transform(Quaternion rotation)
        {
            this.rotation = rotation;
        }

        public Transform(Vector3 position, Quaternion rotation) : this(position)
        {
            this.rotation = rotation;
        }

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
