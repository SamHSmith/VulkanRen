using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Logic.Physics.Shapes;

namespace Fabricor.Main.Logic.Physics
{
    public abstract class Collidable
    {
        public List<IShape> shapes = new List<IShape>();
        public Transform transform;

        public void AddShape(IShape s)
        {
            s.Collidable = this;
            shapes.Add(s);
        }

        public BoundSphere GetBound()
        {
            float r = 0;
            foreach (var s in shapes)
            {
                BoundSphere b = s.ToBoundSphere();
                if (b.radius > r)
                {
                    r = b.radius;
                }
            }
            return new BoundSphere { radius = r, Collidable = this };
        }

        public abstract float GetMass();

        public abstract Vector3 GetPointVelocity(Vector3 worldPoint);


        public void ApplyForce(Vector3 position, Vector3 force)
        {
            ApplyLocalForce((new Transform(position) / this.transform).position, force);
        }

        public abstract void ApplyLocalForce(Vector3 position, Vector3 force);
    }
}
