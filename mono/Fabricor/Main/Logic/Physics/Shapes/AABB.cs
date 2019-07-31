using System;
using System.Numerics;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public struct AABB : IShape
    {
        public Vector3 radii;
        public Vector3 offset;
        public RigidbodyHandle Rigidbody { get; set; }

        public AABB(Transform at,BoundSphere a,Transform bt, BoundSphere b,RigidbodyHandle collidable)
        {
            Vector3 dir = (bt.position - at.position)/2;
            Vector3 center = at.position + dir;
            Vector3 rad = dir;
            rad.X = Math.Abs(rad.X);
            rad.Y = Math.Abs(rad.Y);
            rad.Z = Math.Abs(rad.Z);
            rad += new Vector3(a.radius);
            rad += new Vector3(b.radius);
            radii = rad;
            offset = dir;
            this.Rigidbody = collidable;
        }



        public bool HasImplementation(IShape s)
        {
            if (s is AABB)
                return true;

            if (s is BoundSphere)
                return true;

            return false;
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, IShape other)
        {
            if (other is AABB)
                return IsColliding(at, bt, (AABB)other);

            if (other is BoundSphere)
                return IsColliding(at, bt, (BoundSphere)other);

            return new ContactPoint[0];
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, AABB other)
        {
            Vector3 relpos = bt.position - at.position - offset +other.offset;


            if (relpos.X > radii.X + other.radii.X || relpos.X < -radii.X - other.radii.X)
                return new ContactPoint[0];

            if (relpos.Y > radii.Y + other.radii.Y || relpos.Y < -radii.Y - other.radii.Y)
                return new ContactPoint[0];

            if (relpos.Z > radii.Z + other.radii.Z || relpos.Z < -radii.Z - other.radii.Z)
                return new ContactPoint[0];

            Vector3 pos = Maths.Clamp(relpos, radii * -1, radii)+at.position;
            return new ContactPoint[] { new ContactPoint { position = new Vector3[] { pos },normal=Vector3.Normalize(Maths.SnapVector(relpos)),bodyA=this.Rigidbody,bodyB= other.Rigidbody} };
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, BoundSphere other)
        {
            Vector3 relpos = bt.position - at.position - offset;
            Vector3 closest = Maths.Clamp(relpos, radii * -1, radii);

            if ((relpos - closest).Length() < other.radius)
                return new ContactPoint[] { new ContactPoint {position=new Vector3[]{(relpos / 2)+at.position },
                normal=Vector3.Normalize(closest),
                    bodyA = this.Rigidbody, bodyB = other.Rigidbody } };

            return new ContactPoint[0];
        }

        public AABB ToAABB()
        {
            return this;
        }

        public BoundSphere ToBoundSphere()
        {
            return new BoundSphere { radius = (radii + Vector3.Abs(offset)).Length(),Rigidbody=this.Rigidbody };
        }
    }
}
