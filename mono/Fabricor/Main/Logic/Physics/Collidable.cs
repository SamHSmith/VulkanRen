using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Logic.Physics.Shapes;

namespace Fabricor.Main.Logic.Physics
{
    public abstract class Collidable
    {
        public List<IShape> shapes = new List<IShape>();
        public Transform transform;

        public void AddShape(IShape s)
        {
            s.Collidable = this;
            shapes.Add(s);
        }

        public BoundSphere GetBound()
        {
            float r = 0;
            foreach (var s in shapes)
            {
                BoundSphere b = s.ToBoundSphere();
                if (b.radius > r)
                {
                    r = b.radius;
                }
            }
            return new BoundSphere { radius = r, Collidable = this };
        }

        public abstract float GetMass();

        public abstract float GetInverseMass();

        public abstract Vector3 GetInertia();

        public abstract Vector3 GetInverseInertia();

        public abstract float GetPointInertia(Vector3 worldPoint);

        public abstract Vector3 GetTangentVelocity(Vector3 worldPoint,out Vector3 axis);

        public abstract Vector3 GetLinearVelocity();

        public abstract Vector3 GetAngularVelocity();

        public abstract float GetDistanceToCenterOfMass(Vector3 worldposition);

        public float GetWorldPerpFactor(Vector3 force, Vector3 worldposition)
        {
            Vector3 localpos = Vector3.Transform(worldposition - transform.position,
                Quaternion.Inverse(transform.rotation));
            return GetPerpFactor(Vector3.Transform(force, Quaternion.Inverse(transform.rotation)), localpos);
        }

        public abstract float GetPerpFactor(Vector3 force, Vector3 position);

        public abstract void ApplyAngularAcceleration(Vector3 energy, Vector3 position);

        public abstract void ApplyLinearForce(Vector3 force);

        public float ApplyAcceleration(Vector3 position, Vector3 acceleration, float linearFactor)
        {
            return ApplyLocalAcceleration((new Transform(position) / this.transform).position, acceleration, linearFactor);
        }

        public abstract float ApplyLocalAcceleration(Vector3 position, Vector3 acceleration, float linearFactor);
    }
}
