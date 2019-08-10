using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public class ConvexHull : ICompoundSubShape, ISupportable
    {
        public Vector3 Localposition { get; set; }
        public IShapeRoot root { get; set; }

        private Vector3[] points;
        private BoundSphere bound;

        public ConvexHull(Vector3[] points)
        {
            this.points = points;
        }

        public bool HasImplementation(IShape s)
        {
            if (s is CompoundShape || s is ISupportable)
                return true;

            return false;
        }


        public ContactPoint[] IsColliding(Transform at, Transform bt, IShape other)
        {
            if (other is CompoundShape)
            {
                return other.IsColliding(bt, at, this);
            }
            if(other is ISupportable)
            {
                ContactPoint point= GJKS.GetPenetration(at, bt, this, (ISupportable)other);

                if (point.normal == Vector3.Zero)
                    return new ContactPoint[0];

                point.bodyA = this.root;
                point.bodyB = other.root;
                return new ContactPoint[] { point };
            }
            return new ContactPoint[0];
        }

        public Vector3 Support(Transform t, Vector3 dir)
        {
            float max = float.MinValue;
            Vector3 v = Vector3.Zero;
            foreach (var p in points)
            {
                Vector3 pos = t.position + Vector3.Transform(p, t.rotation);
                float d = Vector3.Dot(dir, pos);
                if (d > max)
                {
                    max = d;
                    v = pos;
                }
            }
            return v;
        }

        public Vector3 GetCenter(Transform t)
        {
            Vector3 Center = Vector3.Zero;
            foreach (var p in points)
            {
                Center += p;
            }
            Center /= points.Length;
            Vector3 worldCenter = t.position + Vector3.Transform(Center, t.rotation);
            return worldCenter;
        }

        public AABB ToAABB()
        {
            return ToBoundSphere().ToAABB();
        }

        public BoundSphere ToBoundSphere()
        {
            if (bound == null)
                UpdateBound();

            return bound;
        }

        public void UpdateBound()
        {
            float max = float.MinValue;
            foreach (var p in points)
            {
                float l = p.Length();
                if (l > max)
                    max = l;
            }
            bound = new BoundSphere(max, root, Localposition);
            root.UpdateBound();
        }
    }
}
