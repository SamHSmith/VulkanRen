using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Logic.Physics.Shapes;

namespace Fabricor.Main.Logic.Physics
{
    public static class Simulation
    {
        public static List<Rigidbody> rigidbodies = new List<Rigidbody>();
        //public static List<Collidable> statics = new List<Collidable>();TODO Add Static class

        public static void TimeStep(float delta)
        {
            Move(delta);
            PerformCollisions(NarrowPhase(BroadPhase()));
        }

        private static void Move(float delta)
        {
            foreach (var rb in rigidbodies)
            {
                rb.transform.position += rb.linearVelocity * delta;
                if(rb.angularVelocity.Length()>0)
                    rb.transform.rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(rb.angularVelocity),
                        rb.angularVelocity.Length()*delta)*rb.transform.rotation;
            }
        }
        private static void PerformCollisions(List<ContactPoint> contacts)
        {

            foreach (var c in contacts)
            {
                float a1 = Vector3.Dot(c.bodyA.GetPointVelocity(c.position), c.normal);
                float b1 = Vector3.Dot(c.bodyB.GetPointVelocity(c.position), c.normal);

                float p = 2*(a1 - b1) / (c.bodyA.GetMass() + c.bodyB.GetMass());

                Console.WriteLine("normal" + c.normal);
                Console.WriteLine("P " + p);

                c.bodyA.ApplyForce(c.position, -c.normal *p* c.bodyB.GetMass() * c.bodyA.GetMass());//Second masses get divided away later
                c.bodyB.ApplyForce(c.position, c.normal *p* c.bodyA.GetMass() * c.bodyB.GetMass());
            }
        }

        private static List<ContactPoint> NarrowPhase(List<CollidablePair> pairs)
        {
            List<ContactPoint> contacts = new List<ContactPoint>();
            foreach (var p in pairs)
            {
                foreach (var a in p.a.shapes)
                {
                    foreach (var b in p.b.shapes)
                    {
                        contacts.AddRange(a.IsColliding(p.a.transform, p.b.transform, b));
                    }
                }
            }
            return contacts;
        }

        private static List<CollidablePair> BroadPhase()
        {
            List<Collidable> collidables = new List<Collidable>();
            collidables.AddRange(rigidbodies);
            //Statics here

            List<IShape> bounds = new List<IShape>();
            foreach (var c in collidables)
            {
                bounds.Add(c.GetBound());
            }

            List<CollidablePair> pairs = new List<CollidablePair>();

            for (int i = 0; i < collidables.Count; i++)
            {
                for (int k = i+1; k < collidables.Count; k++)
                {
                    if (bounds[i].IsColliding(collidables[i].transform, collidables[k].transform, bounds[k]).Length > 0)
                    {
                        CollidablePair pair = new CollidablePair { a = collidables[i], b = collidables[k] };
                        if (!(pair.a is Rigidbody || pair.b is Rigidbody))
                            continue;
                        pairs.Add(pair);
                    }
                }
            }
            return pairs;
        }
    }

    struct CollidablePair
    {
        public Collidable a, b;
    }
}
