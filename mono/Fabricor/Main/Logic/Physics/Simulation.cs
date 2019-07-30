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
                if (rb.angularVelocity.Length() > 0)
                    rb.transform.rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(rb.angularVelocity),
                        rb.angularVelocity.Length() * delta) * rb.transform.rotation;
            }
        }
        private static void PerformCollisions(List<ContactPoint> contacts)
        {

            foreach (var c in contacts)
            {
                Vector3 normal = c.normal;
                int contactCount = c.position.Length;
                Vector3 position = Vector3.Zero;
                foreach (var pos in c.position)
                {
                    position += pos;
                }
                position /= contactCount;

                c.bodyA.transform.position += c.normal * c.depth / 2;
                c.bodyB.transform.position -= c.normal * c.depth / 2;

                float kinlin = Vector3.Dot(normal, c.bodyA.GetLinearVelocity())+ 
                Vector3.Dot(-normal, c.bodyB.GetLinearVelocity());//Kinetic linear energi for body a and body b
                kinlin = kinlin * kinlin;
                kinlin *= c.bodyA.GetMass()+c.bodyB.GetMass();
                kinlin /= 2;


                float tangentspeed1 = Vector3.Dot(normal, c.bodyA.GetTangentVelocity(position, out var axis1));
                float tangentspeed2 = Vector3.Dot(-normal, c.bodyB.GetTangentVelocity(position, out var axis2));

                float angvel1 = tangentspeed1 / c.bodyA.GetDistanceToCenterOfMass(position);
                float angvel2 = tangentspeed2 / c.bodyA.GetDistanceToCenterOfMass(position);


                float kinang = angvel2 - angvel1;
                kinang *= kinang;
                kinang *= Math.Abs(Vector3.Dot(axis1, c.bodyA.GetInertia()))+ Math.Abs(Vector3.Dot(axis2, c.bodyB.GetInertia()));
                kinang /= 2;
                if (float.IsNaN(kinang))
                    kinang = 0;

                Console.WriteLine("Ang Energy " + kinang);
                Console.WriteLine("Lin Energy " + kinlin);
                Console.WriteLine("Normal " + normal);



                float totalEnergy = kinlin+kinang;

                //Angular Code
                float apart = Math.Abs(c.bodyA.GetWorldPerpFactor(Vector3.Normalize(normal), position)/2);
                float bpart = Math.Abs(c.bodyB.GetWorldPerpFactor(Vector3.Normalize(normal), position)/2);

                if (float.IsNaN(apart))
                    apart = 0;
                if (float.IsNaN(bpart))
                    bpart = 0;

                c.bodyA.ApplyAngularAcceleration(-normal * totalEnergy * apart, position);
                c.bodyB.ApplyAngularAcceleration(-normal * totalEnergy * bpart, position);



                totalEnergy *= 1 - (apart + bpart);

                float speed = totalEnergy / ((c.bodyA.GetMass() + c.bodyB.GetMass())/2);

                speed = (float)Math.Sqrt(speed);
                if (float.IsNaN(speed))
                    speed = 0;

                float p = 2 * speed / (c.bodyA.GetMass() + c.bodyB.GetMass());



                c.bodyA.ApplyLinearForce(normal * p*c.bodyB.GetMass()*c.bodyA.GetMass());
                c.bodyB.ApplyLinearForce(normal * -p*c.bodyA.GetMass() * c.bodyB.GetMass());


                /*
                float a1 = Vector3.Dot(c.bodyA.GetPointVelocity(position), normal);
                float b1 = Vector3.Dot(c.bodyB.GetPointVelocity(position), normal);
                float relativeVelocity = (a1 - b1);
                //Console.WriteLine("rel vel " + relativeVelocity);
                //Console.WriteLine("normal " + Vector3.Transform(c.normal,c.bodyA.transform.rotation));

                //Console.WriteLine("depth " + c.depth);

                c.bodyA.transform.position += c.normal * c.depth / 2;
                c.bodyB.transform.position -= c.normal * c.depth / 2;

                if (relativeVelocity > 0)//We dont want to keep objects inside of each other
                    continue;

                float m1 = c.bodyA.GetMass();
                float m2 = c.bodyB.GetMass();

                float p = 2 * relativeVelocity / (m1 + m2);


                //Console.WriteLine("P " + p);

                float totalM = ((c.bodyA.GetMass() * c.bodyA.GetPointVelocity(c.bodyA.transform.position)) +
                    (c.bodyB.GetMass() * c.bodyB.GetPointVelocity(c.bodyB.transform.position))).Length();

                float linear = c.bodyA.ApplyAcceleration(position, normal * -p * m2, 1);//Second masses get divided away later
                c.bodyB.ApplyAcceleration(position, normal * p * m1, linear);
                */
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
                for (int k = i + 1; k < collidables.Count; k++)
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
