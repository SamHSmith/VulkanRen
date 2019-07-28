using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public struct ConvexShape : IShape
    {
        public Collidable Collidable { get; set; }

        public Vector3[] points;

        public Vector3[] planes;

        public ConvexShape(Vector3[] points, Vector3[] planes) : this()
        {
            this.points = points;
            this.planes = planes;
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
            }


            return new ContactPoint[0];
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, ConvexShape other)
        {/*
            List<ContactPoint> contacts = new List<ContactPoint>();
            foreach (var point in points)
            {
                Vector3 local = Vector3.Transform(at.position + Vector3.Transform(point, at.rotation) - bt.position,
                    Quaternion.Inverse(bt.rotation));
                Intersect i = other.IsInside(local);
                if (i!=null)
                {
                    contacts.Add( new ContactPoint {position=Vector3.Transform(i.pos,bt.rotation)+bt.position,
                         normal= Vector3.Transform(i.normal, bt.rotation),
                        depth=i.depth,bodyA=this.Collidable,bodyB=other.Collidable});
                }

            }
            return contacts.ToArray();
            */

            List<Vector3> axis = new List<Vector3>();
            axis.AddRange(this.planes);
            foreach (var plane in other.planes)
            {
                Vector3 worldPlane = Vector3.Transform(plane, bt.rotation);
                Vector3 localPlane= Vector3.Transform(worldPlane, Quaternion.Inverse(at.rotation));
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

            List<Vector3> absAxis = new List<Vector3>();
            foreach (var a in axis)
            {
                absAxis.Add(Vector3.Abs(a));//TODO Remove duplicates
            }
            axis = absAxis;
            foreach (var a in axis)
            {
                //Debug only
                Vector3 worldAxis = Vector3.Transform(a, at.rotation);

                

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

                float max=0, min=0;
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

                if (more && min > minother&&min<=maxother)
                {
                    ainside = true;
                    d = min- maxother;
                }

                if (!more && max < maxother&&max>=minother)
                {
                    ainside = true;
                    d = max-minother;
                }

                float flipFactor = 1;
                if (ainside&&d<0)
                {
                    flipFactor = -1;
                }

                if (d*flipFactor < depth)
                {
                    depth = d * flipFactor;
                    normal = -a * flipFactor;
                    //temp
                    localPoint = normal * depth;
                }

                if (!ainside)
                {
                    return new ContactPoint[] { };//And so, No intersect
                }

            }

            return new ContactPoint[] { new ContactPoint {position= Vector3.Transform(localPoint, at.rotation) + at.position,
                    normal=Vector3.Transform(normal, at.rotation),depth=depth,bodyA=this.Collidable,bodyB=other.Collidable } };
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
            return new AABB { radii = radii, Collidable = this.Collidable };
        }


        public BoundSphere ToBoundSphere()
        {
            float radius = 0;
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
        public Vector3 position, normal;
    }

    public class Intersect
    {
        public Vector3 pos;
        public Vector3 normal;
        public float depth;
    }
}
