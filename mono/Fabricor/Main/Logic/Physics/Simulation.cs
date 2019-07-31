using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Logic.Physics.State;

namespace Fabricor.Main.Logic.Physics
{
    public static class Simulation
    {
        private static PhysicsState state = new PhysicsState(1000);
        private static List<RigidbodyHandle> handles = new List<RigidbodyHandle>();


        internal static Span<RigidbodyState> GetRBState(RigidbodyHandle handle)
        {
            return state.GetRef(handle.handle);
        }

        public static void TimeStep(float delta)
        { 
            Move(delta);

            PerformCollisions(NarrowPhase(BroadPhase()));
        }

        private static void Move(float delta)
        {
            Span<RigidbodyState> span = state.State;
            for(int i=0;i<span.Length;i++)
            {
                span[i].transform.position += span[i].linearVelocity * delta;
                if (span[i].angularVelocity.Length() > 0)
                    span[i].transform.rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(span[i].angularVelocity),
                        span[i].angularVelocity.Length() * delta) * span[i].transform.rotation;
            }
        }
        private static void PerformCollisions(List<ContactPoint> contacts)
        {
            
            foreach (var c in contacts)
            {
                Vector3 normal = -c.normal;
                int contactCount = c.position.Length;
                Vector3 position = Vector3.Zero;
                foreach (var pos in c.position)
                {
                    position += pos;
                }
                position /= contactCount;

                float p = c.depth / (c.bodyA.GetMass() + c.bodyB.GetMass());

                c.bodyA.transform.position += c.normal * p * c.bodyA.GetMass();
                c.bodyB.transform.position -= c.normal * p * c.bodyB.GetMass();

                float e = 1f;

                Vector3 ra = c.bodyA.GetDistanceToCenterOfMass(position);
                Vector3 rb = c.bodyB.GetDistanceToCenterOfMass(position);


                Vector3 pointvel1 = c.bodyA.GetLinearVelocity() + Vector3.Cross(c.bodyA.GetAngularVelocity(), ra);
                Vector3 pointvel2 = c.bodyB.GetLinearVelocity() + Vector3.Cross(c.bodyB.GetAngularVelocity(), rb);


                float j = -(1 + e) * Vector3.Dot(normal, pointvel1-pointvel2);
                j /= c.bodyA.GetInverseMass() + c.bodyB.GetInverseMass() + 
                    (Vector3.Cross(ra, normal) * Vector3.Cross(ra, normal) * c.bodyA.GetInverseInertia()).Length() +
                    (Vector3.Cross(rb, normal)* Vector3.Cross(rb, normal) * c.bodyB.GetInverseInertia()).Length();



                c.bodyA.ApplyLinearForce(j * normal);
                c.bodyB.ApplyLinearForce(-j * normal);

                c.bodyA.ApplyTorque(Vector3.Cross(ra, j * normal));
                c.bodyB.ApplyTorque(Vector3.Cross(rb, -j * normal));


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
                        contacts.AddRange(a.IsColliding(p.a.state[0].transform, p.b.state[0].transform, b));
                    }
                }
            }
            return contacts;
        }

        private static List<CollidablePair> BroadPhase()
        {
            List<RigidbodyHandle> rbs = handles;

            List<IShape> bounds = new List<IShape>();
            foreach (var c in rbs)
            {
                bounds.Add(c.GetBound());
            }

            List<CollidablePair> pairs = new List<CollidablePair>();
            int checks = 0;
            for (int i = 0; i < rbs.Count; i++)
            {
                for (int k = i + 1; k < rbs.Count; k++)
                {
                    checks++;
                    if (bounds[i].IsColliding(rbs[i].state[0].transform, rbs[k].state[0].transform, bounds[k]).Length > 0)
                    {
                        CollidablePair pair = new CollidablePair { a = rbs[i], b = rbs[k] };

                        pairs.Add(pair);
                    }
                }
            }
            Console.WriteLine(checks);
            return pairs;
        }
    }

    struct CollidablePair
    {
        public RigidbodyHandle a, b;
    }
}
