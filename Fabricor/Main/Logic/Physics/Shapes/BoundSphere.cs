using System;
using System.Numerics;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public class BoundSphere : IShape
    {
        public float radius;

        public Vector3 CompoundOffset=Vector3.Zero;

        public IShapeRoot root { get; set; }

        public BoundSphere(float radius)
        {
            this.radius = radius;
        }

        public BoundSphere(float radius, IShapeRoot root) : this(radius)
        {
            this.root = root;
        }

        public BoundSphere(float radius, IShapeRoot root, Vector3 localposition) : this(radius, root)
        {
            CompoundOffset = localposition;
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
                return new ContactPoint[] { new ContactPoint { position=Maths.Average(at.position, bt.position) , normal = dir,
                    bodyA = this.root, bodyB = other.root } };

            return new ContactPoint[0];
        }

        public AABB ToAABB()
        {
            return new AABB(Vector3.One * radius,root);
        }

        public BoundSphere ToBoundSphere()
        {
            return this;
        }

        public Vector3 CenterOfMass()
        {
            return Vector3.Zero;
        }
    }
}
