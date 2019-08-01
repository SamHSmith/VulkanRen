using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Logic.Physics.State;
using Fabricor.Main.Toolbox;

namespace Fabricor.Main.Logic.Physics
{
    public static class Simulation
    {
        private static PhysicsState editState = new PhysicsState(10000);
        private static PhysicsState oldState = new PhysicsState(10000);
        private static PhysicsState interpolatedState = new PhysicsState(10000);
        private static PhysicsState newState = new PhysicsState(10000);
        private static List<RigidbodyHandle> handles = new List<RigidbodyHandle>();


        internal static Span<RigidbodyState> GetRBState(RigidbodyHandle handle)
        {
            return editState.GetRef(handle.handle);
        }

        internal static Span<RigidbodyState> GetRBInterpolatedState(RigidbodyHandle handle)
        {
            return interpolatedState.GetRef(handle.handle);
        }

        public static RigidbodyHandle GetNewRigidbody()
        {
            Span<RigidbodyState> span = editState.Span;
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

        public unsafe static void UpdateInterpolation(float t,float fixeddelta)
        {
            oldState.state.CopyTo(interpolatedState.state);

            RigidbodyState* iptr = (RigidbodyState*)interpolatedState.state.ptr;
            RigidbodyState* nptr = (RigidbodyState*)newState.state.ptr;

            for (int i = 0; i < interpolatedState.state.length; i++)
            {
                if ((*nptr).IsAssigned)
                {
                    (*iptr).transform.position = Hermite((*iptr).transform.position, (*iptr).linearVelocity* fixeddelta,
                        (*nptr).transform.position, (*nptr).linearVelocity* fixeddelta, t);
                    (*iptr).transform.rotation = Quaternion.Lerp((*iptr).transform.rotation, (*nptr).transform.rotation, t);
                }
                iptr++;
                nptr++;
            }
        }

        public static Vector3 Hermite(
            Vector3 value1,
            Vector3 tangent1,
            Vector3 value2,
            Vector3 tangent2,
            float amount
        )
        {
            Vector3 result = new Vector3();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        public static void Hermite(
            ref Vector3 value1,
            ref Vector3 tangent1,
            ref Vector3 value2,
            ref Vector3 tangent2,
            float amount,
            out Vector3 result
        )
        {
            result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
            result.Z = MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
        }

        static int frame = 0;
        public static void TimeStep(float delta)
        {
            Stopwatch frametime = new Stopwatch();
            frametime.Start();



            Move(delta);

            Stopwatch s = new Stopwatch();

            s.Start();
            List<CollidablePair> broad = BroadPhase();
            s.Stop();
            long broadTime = s.ElapsedMilliseconds;
            s.Reset();

            s.Start();
            List<ContactPoint> narrow = NarrowPhase(broad);
            s.Stop();
            long narrowTime = s.ElapsedMilliseconds;
            s.Reset();

            s.Start();
            PerformCollisions(narrow);
            s.Stop();
            frametime.Stop();

            Console.WriteLine("Physics Frame " + frame + ", Broadtime: "+broadTime+", Narrowtime: "+narrowTime+", Performtime: "+s.ElapsedMilliseconds);
            Console.WriteLine("Frametime: " + frametime.ElapsedMilliseconds);
            frame++;
        }

        public static void SwapBuffers()
        {
            //Swap buffers
            newState.state.CopyTo(oldState.state);//Move back buffer
            PhysicsState oldComplete = newState;
            newState = editState;
            editState = oldComplete;
            newState.state.CopyTo(editState.state);
        }

        private static void Move(float delta)
        {
            Span<RigidbodyState> span = editState.Span;
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

                float e = 1f;

                Vector3 ra = spana[0].GetDistanceToCenterOfMass(position);
                Vector3 rb = spanb[0].GetDistanceToCenterOfMass(position);


                Vector3 pointvel1 = spana[0].GetLinearVelocity() + Vector3.Cross(spana[0].GetAngularVelocity(), ra);
                Vector3 pointvel2 = spanb[0].GetLinearVelocity() + Vector3.Cross(spanb[0].GetAngularVelocity(), rb);


                float j = -(1 + e) * Vector3.Dot(normal, pointvel1-pointvel2);
                j /= spana[0].GetInverseMass() + spanb[0].GetInverseMass() + 
                    (Vector3.Cross(ra, normal) * Vector3.Cross(ra, normal) * spana[0].GetInverseInertia()).Length() +
                    (Vector3.Cross(rb, normal)* Vector3.Cross(rb, normal) * spanb[0].GetInverseInertia()).Length();




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

            List<AABBMarker> markers = new List<AABBMarker>(handles.Count*2);
            for (int i = 0; i < handles.Count; i++)
            {
                AABB b=handles[i].GetBound().ToAABB();
                markers.Add(new MinAABB {rb=handles[i],position= b.WorldMin(handles[i].state[0].transform.position) });
                markers.Add(new MaxAABB { rb = handles[i], position = b.WorldMax(handles[i].state[0].transform.position) });
            }
            List<CollidablePair> pairs = new List<CollidablePair>();

            markers.Sort((x, y) => x.position.X.CompareTo(y.position.X));
            Prune(ref markers);

            markers.Sort((x, y) => x.position.Y.CompareTo(y.position.Y));
            Prune(ref markers);

            markers.Sort((x, y) => x.position.Z.CompareTo(y.position.Z));
            Prune(ref markers);

            //Find collisions

            /*

            for (int i = 0; i < rbs.Count; i++)
            {
                for (int k = i + 1; k < rbs.Count; k++)
                {
                    if (bounds[i].IsColliding(rbs[i].state[0].transform, rbs[k].state[0].transform, bounds[k]).Length > 0)
                    {
                        CollidablePair pair = new CollidablePair { a = rbs[i], b = rbs[k] };

                        pairs.Add(pair);
                    }
                }
            }
            */
            return pairs;
        }

        private static void Prune(ref List<AABBMarker> markers)
        {
            int last = 0;
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i] is MinAABB)
                {
                    last = i;
                }
                else
                {
                    if(markers[last]is MinAABB)
                    if (markers[i].rb == ((MinAABB)markers[last]).rb)
                    {
                        markers.RemoveAt(i);
                        markers.RemoveAt(last);
                    }
                }
            }
        }

        public static void CleanUp()
        {
            editState.CleanUp();
            newState.CleanUp();
            oldState.CleanUp();
        }
    }

    struct CollidablePair
    {
        public RigidbodyHandle a, b;
    }

    interface AABBMarker
    {
        RigidbodyHandle rb { get; set; }
        Vector3 position { get; set; }
}

    struct MinAABB : AABBMarker
    {
        public RigidbodyHandle rb { get; set; }
        public Vector3 position { get; set; }
    }

    struct MaxAABB : AABBMarker
    {
        public RigidbodyHandle rb { get; set; }
        public Vector3 position { get; set; }
    }
}
