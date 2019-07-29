using System;
using System.Numerics;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public struct BoundSphere : IShape
    {
        public float radius;

        public Collidable Collidable { get; set; }

        public BoundSphere(float radius) : this()
        {
            this.radius = radius;
        }

        public bool HasImplementation(IShape s)
        {
            if (s is BoundSphere)
                return true;

            return false;
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, IShape other)
        {
            if (other is BoundSphere)
                return IsColliding(at, bt, (BoundSphere) other);

            return new ContactPoint[0];
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, BoundSphere other)
        {
            Vector3 dir = (at.position - bt.position);
            if (dir.Length() < this.radius + other.radius)
                return new ContactPoint[] { new ContactPoint { position=new Vector3[]{Maths.Average(at.position, bt.position) }, normal = dir,
                    bodyA = this.Collidable, bodyB = other.Collidable } };

            return new ContactPoint[0];
        }

        public AABB ToAABB()
        {
            return new AABB { radii = new Vector3(radius, radius, radius),Collidable=this.Collidable };
        }

        public BoundSphere ToBoundSphere()
        {
            return this;
        }
    }
}
