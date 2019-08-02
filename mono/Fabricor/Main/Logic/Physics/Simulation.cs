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
        private static PhysicsState editState = new PhysicsState(1000);
        private static PhysicsState oldState = new PhysicsState(1000);
        private static PhysicsState interpolatedState = new PhysicsState(1000);
        private static PhysicsState newState = new PhysicsState(1000);
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

        public unsafe static void UpdateInterpolation(float t, float fixeddelta)
        {
            oldState.state.CopyTo(interpolatedState.state);

            RigidbodyState* iptr = (RigidbodyState*)interpolatedState.state.ptr;
            RigidbodyState* nptr = (RigidbodyState*)newState.state.ptr;

            for (int i = 0; i < interpolatedState.state.length; i++)
            {
                if ((*nptr).IsAssigned)
                {
                    (*iptr).transform.position = Hermite((*iptr).transform.position, (*iptr).linearVelocity * fixeddelta,
                        (*nptr).transform.position, (*nptr).linearVelocity * fixeddelta, t);
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

            Console.WriteLine("Physics Frame " + frame + ", Broadtime: " + broadTime + ", Narrowtime: " + narrowTime + ", Performtime: " + s.ElapsedMilliseconds);
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
            for (int i = 0; i < span.Length; i++)
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


                float j = -(1 + e) * Vector3.Dot(normal, pointvel1 - pointvel2);
                j /= spana[0].GetInverseMass() + spanb[0].GetInverseMass() +
                    (Vector3.Cross(ra, normal) * Vector3.Cross(ra, normal) * spana[0].GetInverseInertia()).Length() +
                    (Vector3.Cross(rb, normal) * Vector3.Cross(rb, normal) * spanb[0].GetInverseInertia()).Length();




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
                RigidbodyHandle a = (RigidbodyHandle)p.a;
                RigidbodyHandle b = (RigidbodyHandle)p.b;

                contacts.AddRange(a.shape.IsColliding(a.state[0].transform, b.state[0].transform, b.shape));

            }
            return contacts;
        }

        private static List<CollidablePair> BroadPhase()
        {

            List<AABB> aABBs = new List<AABB>();
            for (int i = 0; i < handles.Count; i++)
            {
                AABB b = handles[i].GetBound().ToAABB();
                aABBs.Add(b);
            }

            List<CollidablePair> pairsfinal = new List<CollidablePair>();
            List<CollidablePair> pairs = SweepAndPrune(aABBs);

            for (int i = 0; i < pairs.Count; i++)
            {
                RigidbodyHandle a = (RigidbodyHandle)pairs[i].a;
                RigidbodyHandle b = (RigidbodyHandle)pairs[i].b;

                if (!a.state[0].IsAssigned || !b.state[0].IsAssigned)//TODO add static flags
                    continue;

                if((a.state[0].transform.position-b.state[0].transform.position).Length()<a.shape.ToBoundSphere().radius+b.shape.ToBoundSphere().radius)
                {
                    pairsfinal.Add(new CollidablePair {a=a,b=b });
                }
            }

            return pairsfinal;
        }

        public static List<CollidablePair> SweepAndPrune(List<AABB> aABBs)
        {
            List<AABBMarker> markersx = new List<AABBMarker>(aABBs.Count * 2);
            for (int i = 0; i < aABBs.Count; i++)
            {
                AABB b = aABBs[i];
                markersx.Add(new MinAABB { a = aABBs[i], position = b.WorldMin(handles[i].state[0].transform.position) });
                markersx.Add(new MaxAABB { a = aABBs[i], position = b.WorldMax(handles[i].state[0].transform.position) });
            }


            markersx.Sort((x, y) => x.position.X.CompareTo(y.position.X));
            Prune(markersx,out var markersy);

            markersy.Sort((x, y) => x.position.Y.CompareTo(y.position.Y));
            Prune(markersy, out var markersz);

            markersz.Sort((x, y) => x.position.Z.CompareTo(y.position.Z));
            Prune(markersz, out var markersfinal);

            List<CollidablePair> pairs = new List<CollidablePair>();

            AABB last=null;
            List<AABB> open = new List<AABB>();
            for (int i = 0; i < markersfinal.Count; i++)
            {
                if(markersfinal[i] is MinAABB)
                {
                    last = ((MinAABB)markersfinal[i]).a;
                    open.Add(last);
                }
                else
                {
                    if (markersfinal[i].a == last)
                    {
                        open.Remove(last);
                    }
                    foreach (var aa in open)
                    {
                        pairs.Add(new CollidablePair {a=aa.root,b= markersfinal[i].a.root });
                    }
                    if (open.Contains(markersfinal[i].a))
                    {
                        open.Remove(markersfinal[i].a);
                    }
                }
            }
            return pairs;
        }

        private static void Prune(List<AABBMarker> markers, out List<AABBMarker> newmarkers)
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
                    if (markers[last] is MinAABB)
                        if (markers[i].a == ((MinAABB)markers[last]).a)
                        {
                            markers.RemoveAt(i);
                            markers.RemoveAt(last);
                        }
                }
            }
            newmarkers = new List<AABBMarker>(markers);
        }

        public static void CleanUp()
        {
            editState.CleanUp();
            newState.CleanUp();
            oldState.CleanUp();
        }
    }

    public struct CollidablePair
    {
        public IShapeRoot a, b;
    }

    interface AABBMarker
    {
        AABB a { get; set; }
        Vector3 position { get; set; }
    }

    struct MinAABB : AABBMarker
    {
        public AABB a { get; set; }
        public Vector3 position { get; set; }
    }

    struct MaxAABB : AABBMarker
    {
        public AABB a { get; set; }
        public Vector3 position { get; set; }
    }
}
