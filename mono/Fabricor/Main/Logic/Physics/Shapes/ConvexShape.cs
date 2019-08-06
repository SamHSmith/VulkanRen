using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public class ConvexShape : ICompoundSubShape
    {
        public IShapeRoot root { get; set; }
        public Vector3 Localposition { get; set; }

        public Vector3[] points;

        public Vector3[] planes;

        public const float margin = 0.02f;

        public float frictionFactor;

        public ConvexShape(Vector3[] points, Vector3[] planes)
        {
            this.points = points;
            this.planes = planes;
            frictionFactor = 0.5f;
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

            List<Vector3> otherlocal = new List<Vector3>();
            foreach (var otherpoint in other.points)
            {
                Vector3 local = Vector3.Transform(bt.position + Vector3.Transform(otherpoint, bt.rotation) - at.position,
                Quaternion.Inverse(at.rotation));
                otherlocal.Add(local);
            }

            List<Vector3> axis = new List<Vector3>();
            axis.AddRange(this.planes);
            foreach (var plane in other.planes)
            {
                Vector3 worldPlane = Vector3.Transform(plane, bt.rotation);
                Vector3 localPlane = Vector3.Transform(worldPlane, Quaternion.Inverse(at.rotation));
                axis.Add(localPlane);
            }

            List<Vector3> edges = new List<Vector3>();
            for (int i = 0; i < axis.Count; i++)
            {
                for (int k = i + 1; k < axis.Count; k++)
                {
                    Vector3 edge = axis[i] + axis[k];
                    if (edge.Length() > float.Epsilon)
                    {
                        edges.Add(Vector3.Normalize(edge));
                    }
                }
            }
            axis.AddRange(edges);

            float depth = float.MaxValue;
            Vector3 localPoint = Vector3.Zero;
            Vector3 normal = Vector3.Zero;



            //axis.Add(Vector3.Transform(Vector3.UnitY, Quaternion.Inverse(at.rotation)));

            List<Vector3> meInsideother = new List<Vector3>();
            meInsideother.AddRange(points);
            List<Vector3> otherInsideme = new List<Vector3>();
            otherInsideme.AddRange(otherlocal);

            foreach (var a in axis)
            {
                //Debug only
                Vector3 worldAxis = Vector3.Transform(a, at.rotation);

                if (Vector3.Abs(Maths.SnapVector(worldAxis)) == Vector3.UnitX)
                {

                }

                float minother = float.MaxValue, maxother = float.MinValue;
                foreach (var otherpoint in other.points)
                {
                    Vector3 local = Vector3.Transform(bt.position + Vector3.Transform(otherpoint, bt.rotation) - at.position,
                    Quaternion.Inverse(at.rotation));

                    float value = Vector3.Dot(a, local);
                    if (value > maxother)
                        maxother = value;
                    if (value < minother)
                        minother = value;
                }


                bool ainside = false;

                float max = 0, min = 0;
                foreach (var mypoint in points)
                {

                    float value = Vector3.Dot(a, mypoint);
                    if (value > max)
                        max = value;
                    if (value < min)
                        min = value;
                }

                //calculate values
                float center, centerother;
                center = (max + min) / 2;
                centerother = (maxother + minother) / 2;
                bool more = center > centerother;

                float d = float.MaxValue;

                if (more && min > minother && min <= maxother)
                {
                    ainside = true;
                    d = min - maxother;
                }

                if (!more && max < maxother && max >= minother)
                {
                    ainside = true;
                    d = max - minother;
                }

                if (max >= maxother && min <= minother)//We encapsulate other
                {
                    ainside = true;
                    if (more)
                    {
                        d = min - maxother;
                    }
                    else
                    {
                        d = max - minother;
                    }
                }

                float flipFactor = 1;
                if (ainside && d < 0)
                {
                    flipFactor = -1;
                }

                if (d * flipFactor < depth)
                {
                    depth = d * flipFactor;
                    normal = -a * flipFactor;
                }


                //Contact point generation

                for (int i = 0; i < otherInsideme.Count; i++)
                {
                    float op = Vector3.Dot(a, otherInsideme[i]);
                    if (op > max || op < min)// Is outside me
                        otherInsideme.RemoveAt(i);
                }

                for (int i = 0; i < meInsideother.Count; i++)
                {
                    float mp = Vector3.Dot(a, meInsideother[i]);
                    if (mp > maxother || mp < minother)// Is outside other
                        meInsideother.RemoveAt(i);
                }

                if (!ainside)
                {
                    return new ContactPoint[] { };//And so, No intersect
                }

            }



            List<Vector3> contactPoints = new List<Vector3>();

            foreach (var contact in otherInsideme)
            {
                contactPoints.Add(Vector3.Transform(contact, at.rotation) + at.position);
            }
            foreach (var contact in meInsideother)
            {
                contactPoints.Add(Vector3.Transform(contact, at.rotation) + at.position);

            }
            for (int i = 0; i < contactPoints.Count; i++)
            {
                for (int k = i + 1; k < contactPoints.Count; k++)
                {
                    if (Vector3.Abs(contactPoints[i] - contactPoints[k]).Length() < margin)
                    {
                        contactPoints.RemoveAt(k);
                    }
                }
            }

            if(contactPoints.Count<1)
                return new ContactPoint[] { };//And so, No intersect

            float friction = this.frictionFactor * other.frictionFactor;
            bool nofriction = Math.Abs(friction) < float.Epsilon;

            ContactPoint[] cps;

            if (nofriction)
                cps = new ContactPoint[1];
            else
                cps = new ContactPoint[3];

            ContactPoint cp = (new ContactPoint
            {
                position = contactPoints.ToArray(),
                normal = Vector3.Normalize(Vector3.Transform(normal, at.rotation)),
                depth = depth,
                bodyA = this.root,
                bodyB = other.root
            });
            cps[0] = cp;//Normal contact

            if (!nofriction)
            {
                Vector3 frictionVecA = Vector3.Normalize(Vector3.Cross(cp.normal, cp.normal+Vector3.UnitX));
                Vector3 frictionVecB = Vector3.Normalize(Vector3.Cross(cp.normal, frictionVecA));
                cp.normal = frictionVecA*friction;
                cp.depth = 0;
                cps[1] = cp;
                cp.normal = frictionVecB * friction;
                cps[2] = cp;
            }

            return cps;
        }
        /*
        public Intersect IsInside(Vector3 point)
        {
            float shortestDepth = float.MaxValue;
            Intersect shortestInter = new Intersect { };
            foreach (var p in planes)
            {
                Vector3 local = point - p.position;
                float depth = Vector3.Dot(-p.normal, local);

                if (depth < 0)//If depth is negative then the point is outside the convex shape
                {
                    return null;
                }

                if (depth < shortestDepth && Math.Abs(depth) > float.Epsilon)
                {
                    shortestDepth = depth;
                    shortestInter.normal = p.normal;
                }
            }
            //if the loop completes we have a valid intersect.
            shortestInter.depth = shortestDepth;
            shortestInter.pos = point;
            //Console.WriteLine("depth " + shortestDepth);
            return shortestInter;
        }
        */

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
            return new BoundSphere(radius,root,Localposition);
        }

        public void UpdateBound()
        {
            root.UpdateBound();
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
