using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public struct ConvexShape : IShape
    {
        public Collidable Collidable { get; set; }

        public Vector3[] points;

        public Plane[] planes;



        public bool HasImplementation(IShape s)
        {
            if (s is ConvexShape)
                return true;

            return false;
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, IShape other)
        {
            if (other is ConvexShape)
                return IsColliding(at, bt, (ConvexShape)other);

            return new ContactPoint[0];
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, ConvexShape other)
        {
            foreach (var point in points)
            {
                Vector3 local = Vector3.Transform(at.position + Vector3.Transform(point, at.rotation) - bt.position,
                    Quaternion.Inverse(bt.rotation));

                if (other.IsInside(local))
                {
                    return new ContactPoint[] { new ContactPoint {position=Vector3.Transform(local,bt.rotation)+bt.position,
                         } };
                }
            }

            return new ContactPoint[0];
        }

        public bool IsInside(Vector3 point)
        {
            return false;
        }

        public AABB ToAABB()
        {
            float minx = 0, miny = 0, minz = 0;
            float maxx = 0, maxy = 0, maxz = 0;
            foreach (var p in points)
            {
                if (p.X < minx)
                    minx = p.X;
                if (p.X > maxx)
                    maxx = p.X;
                if (p.Y < miny)
                    miny = p.Y;
                if (p.Y > maxy)
                    maxy = p.Y;
                if (p.Z < minz)
                    minz = p.Z;
                if (p.Z > maxz)
                    maxz = p.Z;
            }
            Vector3 min = new Vector3(minx, miny, minz);
            Vector3 max = new Vector3(maxx, maxy, maxz);
            Vector3 offset = (min + max) / 2;
            Vector3 radii = (max - min) / 2;
            return new AABB { radii = radii,Collidable=this.Collidable };
        }


        public BoundSphere ToBoundSphere()
        {
            float radius=0;
            foreach (var p in points)
            {
                if (p.Length() > radius)
                    radius = p.Length();
            }
            return new BoundSphere { radius = radius, Collidable = this.Collidable };
        }
    }

    public struct Plane
    {
        public Vector3 position,normal;
    }
}
