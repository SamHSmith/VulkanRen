using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public class CompoundShape : ICompoundSubShape, IShapeRoot
    {
        public IShapeRoot root { get; set; }
        public Vector3 Localposition { get; set; }
        public float Mass { get; set; }

        public List<ICompoundSubShape> shapes = new List<ICompoundSubShape>();

        public bool HasImplementation(IShape s)
        {
            return true;
        }

        public ContactPoint[] IsColliding(Transform at, Transform bt, IShape other)
        {
            List<ContactPoint> cps = new List<ContactPoint>();
            foreach (var sh in shapes)
            {
                Transform a = at;
                a.position += Vector3.Transform(sh.Localposition, at.rotation);
                BoundSphere sb = sh.ToBoundSphere();
                Transform thisT = at;
                thisT.position+= Vector3.Transform(sb.CompoundOffset, at.rotation);
                if (sb.IsColliding(thisT, bt, other.ToBoundSphere()).Length > 0)
                {
                    cps.AddRange(sh.IsColliding(a, bt, other));
                }
            }
            for (int i = 0; i < cps.Count; i++)
            {
                ContactPoint cp = cps[i];
                if (cp.bodyA == this)
                {
                    cp.bodyA = root;
                }
                if (cp.bodyB == this)
                {
                    cp.bodyB = root;
                }
                cps[i] = cp;
            }
            return cps.ToArray();
        }

        public AABB ToAABB()
        {
            return ToBoundSphere().ToAABB();
        }

        public BoundSphere ToBoundSphere()
        {
            List<BoundSphere> bounds = new List<BoundSphere>();
            float totalradius = 0;
            foreach (var sh in shapes)
            {
                bounds.Add(sh.ToBoundSphere());
                totalradius += bounds[bounds.Count - 1].radius;
            }
            Vector3 center = Vector3.Zero;
            foreach (var b in bounds)
            {
                center += b.CompoundOffset / totalradius * b.radius;
            }

            float maxr = 0;
            foreach (var s in bounds)
            {
                float r = s.ToBoundSphere().radius + (s.CompoundOffset - center).Length();
                if (r > maxr)
                    maxr = r;
            }
            if (root is RigidbodyHandle)
            {
                return new BoundSphere(maxr + (Localposition + center).Length(), root);
            }
            return new BoundSphere(maxr, root, center + Localposition);
        }

        public void UpdateBound()
        {
            root.UpdateBound();
        }

        public Vector3 CenterOfMass()
        {
            Vector3 sum = Vector3.Zero;
            float totalmass = 0;
            for (int i = 0; i < shapes.Count; i++)
            {
                sum += (shapes[i].CenterOfMass()+shapes[i].Localposition) * shapes[i].Mass;
                totalmass += shapes[i].Mass;
            }
            Vector3 final = sum / totalmass;
            Mass = totalmass;

            if (float.IsNaN(final.Length()))
            {
                final = Vector3.Zero;
            }
            return final;
        }
    }
}
