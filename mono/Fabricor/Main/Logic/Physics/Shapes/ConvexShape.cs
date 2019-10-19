using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public class ConvexShape : ICompoundSubShape,ISupportable
    {
        public IShapeRoot root { get; set; }
        public Vector3 Localposition { get; set; }
        public float Mass { get; set; } = 1;

        public Vector3[] points;

        public const float margin = 0.02f;

        public float frictionFactor=0.4f;

        public ConvexShape(Vector3[] points)
        {
            this.points = points;
        }

        public bool HasImplementation(IShape s)
        {
            if (s is ConvexShape)
                return true;

            return false;
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, IShape other)
        {
            if (other is ConvexShape)
            {
                List<ContactPoint> cntcts = new List<ContactPoint>();
                cntcts.AddRange(IsColliding(at, bt, (ConvexShape)other));
                return cntcts.ToArray();
            }else if(other is CompoundShape)
            {
                List<ContactPoint> cntcts = new List<ContactPoint>();
                cntcts.AddRange(other.IsColliding(bt, at, this));
                return cntcts.ToArray();
            }



            return new ContactPoint[0];
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, ConvexShape other)
        {

            float depth = GJK.DoGJK(this, other, at, bt, out var normal, out var pos);

            if (depth < 0)
            {
                return new ContactPoint[] { };
            }
            else
            {
                float friction = this.frictionFactor * other.frictionFactor;
                bool nofriction = Math.Abs(friction) < float.Epsilon;

                ContactPoint[] cps;

                if (nofriction)
                    cps = new ContactPoint[1];
                else
                    cps = new ContactPoint[3];

                ContactPoint cp = new ContactPoint
                {
                    position = pos,
                    normal = normal,
                    depth = depth,
                    bodyA = this.root,
                    bodyB = other.root
                };
                cps[0] = cp;//Normal contact

                if (!nofriction)
                {
                    Vector3 frictionVecA = Vector3.Normalize(Vector3.Cross(cp.normal, cp.normal + Vector3.UnitX));
                    Vector3 frictionVecB = Vector3.Normalize(Vector3.Cross(cp.normal, frictionVecA));
                    cp.normal = frictionVecA * friction;
                    cp.depth = 0;
                    cps[1] = cp;
                    cp.normal = frictionVecB * friction;
                    cps[2] = cp;
                }

                return cps;
            }
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
            return new AABB(radii, root);
        }


        public BoundSphere ToBoundSphere()
        {
            float radius = 0;
            foreach (var p in points)
            {
                if (p.Length() > radius)
                    radius = p.Length();
            }
            return new BoundSphere(radius, root, Localposition);
        }

        public Vector3 GetFurthestPointInDirection(Vector3 dir, Transform t)
        {
            float maxdot = float.MinValue;
            Vector3 maxpoint = float.MinValue * dir;
            for (int i = 0; i < points.Length; i++)
            {

                Vector3 p = t.position + Vector3.Transform(points[i], t.rotation);
                float dot = Vector3.Dot(dir, p);
                if (dot > maxdot)
                {
                    maxdot = dot;
                    maxpoint = p;
                }
            }
            return maxpoint;
        }

        public void UpdateBound()
        {
            root.UpdateBound();
        }

        public Vector3 CenterOfMass()
        {
            Vector3 pointsum = Vector3.Zero;
            for (int i = 0; i < points.Length; i++)
            {
                pointsum += points[i];
            }
            Vector3 final= pointsum /= points.Length;
            if (float.IsNaN(final.Length()))
            {
                final = Vector3.Zero;
            }
            return final;
        }
    }

    public struct Plane
    {
        public Vector3 position, normal;
    }

    public class Intersect
    {
        public Vector3 pos;
        public Vector3 normal;
        public float depth;
    }
}
