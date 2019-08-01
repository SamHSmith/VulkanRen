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

        public static RigidbodyHandle GetNewRigidbody()
        {
            Span<RigidbodyState> span = GetFullState();
            for (int i = 0; i < span.Length; i++)
            {

                if (!span[i].IsAssigned)
                {
                    span[i] = new RigidbodyState();
                    span[i].IsAssigned = true;
                    span[i].transform = new Transform(Vector3.Zero);
                    span[i].mass = 1;
                    span[i].inertia = Vector3.One;
                    RigidbodyHandle handle = new RigidbodyHandle((uint)i);
                    handles.Add(handle);
                    return handle;
                }

            }

            throw new NotImplementedException("Expanding physics state size not implemented");
        }

        public static Span<RigidbodyState> GetFullState()
        {
            return state.State;
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

                if (float.IsNaN(normal.Length()))
                {
                    continue;
                }

                Span<RigidbodyState> spana = c.bodyA.state;
                Span<RigidbodyState> spanb = c.bodyB.state;

                float p = c.depth / (spana[0].GetMass() + spanb[0].GetMass());

                spana[0].transform.position += c.normal * p * spana[0].GetMass();
                spanb[0].transform.position -= c.normal * p * spanb[0].GetMass();

                float e = 0.2f;

                Vector3 ra = spana[0].GetDistanceToCenterOfMass(position);
                Vector3 rb = spanb[0].GetDistanceToCenterOfMass(position);


                Vector3 pointvel1 = spana[0].GetLinearVelocity() + Vector3.Cross(spana[0].GetAngularVelocity(), ra);
                Vector3 pointvel2 = spanb[0].GetLinearVelocity() + Vector3.Cross(spanb[0].GetAngularVelocity(), rb);


                float j = -(1 + e) * Vector3.Dot(normal, pointvel1-pointvel2);
                j /= spana[0].GetInverseMass() + spanb[0].GetInverseMass() + 
                    (Vector3.Cross(ra, normal) * Vector3.Cross(ra, normal) * spana[0].GetInverseInertia()).Length() +
                    (Vector3.Cross(rb, normal)* Vector3.Cross(rb, normal) * spanb[0].GetInverseInertia()).Length();
                /*
                if (float.IsNaN(j))
                {
                TODO remove
                    PerformCollisions(new List<ContactPoint>(new ContactPoint[] {c }));
                }*/



                spana[0].ApplyLinearForce(j * normal);
                spanb[0].ApplyLinearForce(-j * normal);

                spana[0].ApplyTorque(Vector3.Cross(ra, j * normal));
                spanb[0].ApplyTorque(Vector3.Cross(rb, -j * normal));


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

        public static void CleanUp()
        {
            state.CleanUp();
        }
    }

    struct CollidablePair
    {
        public RigidbodyHandle a, b;
    }
}
