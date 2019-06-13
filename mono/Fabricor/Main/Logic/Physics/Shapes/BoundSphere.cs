using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public struct BoundSphere : IShape
    {
        public float radius;

        public bool HasImplementation(IShape s)
        {
            if (s is BoundSphere)
                return true;

            return false;
        }

        public bool IsColliding(Transform at, Transform bt, IShape other)
        {
            if (other is BoundSphere)
                IsColliding(at, bt, (BoundSphere) other);

            return false;
        }

        public bool IsColliding(Transform at, Transform bt, BoundSphere other)
        {
            if ((at.position - bt.position).Length() < this.radius + other.radius)
                return true;

            return false;
        }

        public AABB ToAABB()
        {
            return new AABB { radii = new Vector3(radius, radius, radius) };
        }

        public BoundSphere ToBoundSphere()
        {
            return this;
        }
    }
}
