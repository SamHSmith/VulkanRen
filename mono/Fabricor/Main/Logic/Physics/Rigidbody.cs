using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public class Rigidbody : Collidable
    {

        public Vector3 linearVelocity;
        public Vector3 angularVelocity;
        public float mass = 1;
        public Vector3 inertia = Vector3.One;

        public Rigidbody()
        {
            transform = new Transform(new Vector3());
        }

        public override void ApplyAngularAcceleration(Vector3 energy, Vector3 position)
        {
            Vector3 axis = Vector3.Normalize(Vector3.Cross(energy, position));

            float torque = GetPerpFactor(energy, position)*position.Length();

            Vector3 angChange = (axis*GetInverseInertia())*torque;

            angularVelocity += angChange;
        }

        public override void ApplyLinearForce(Vector3 force)
        {
            linearVelocity += force * GetInverseMass();
        }

        public override float ApplyLocalAcceleration(Vector3 position, Vector3 acceleration, float linearFactor)
        {
            //TODO add center of mass
            //Console.WriteLine("Force " + acceleration);


            Vector3 axis = Vector3.Normalize(Vector3.Cross(acceleration, position));
            //Console.WriteLine("Axis " + axis);
            //Console.WriteLine("Force Position" + acceleration+" "+position);




            float perpPart = Vector3.Dot(Vector3.Normalize(acceleration), (Vector3.Normalize(Vector3.Cross(-position, axis))));

            float speedChange = acceleration.Length() * perpPart;

            Vector3 angChange = (axis * speedChange / position.Length());

            angularVelocity += angChange;

            //Console.WriteLine("perp " + perpPart);
            //Console.WriteLine("spdChange " + speedChange);
            //Console.WriteLine("angChange " + angChange);

            if (float.IsNaN(perpPart))
            {
                perpPart = 0;
            }

            if (Math.Abs(linearFactor - 1f) < float.Epsilon)//better version of linearfactor == 1
            {
                linearFactor = 1 - Math.Abs(perpPart);
            }

            linearVelocity += acceleration;
            return linearFactor;
        }

        public override Vector3 GetAngularVelocity()
        {
            return angularVelocity;
        }

        public override float GetDistanceToCenterOfMass(Vector3 worldposition)
        {
            return (worldposition - transform.position).Length();
        }

        public override Vector3 GetInertia()
        {
            return inertia;
        }

        public override Vector3 GetInverseInertia()
        {
            if (inertia.Length() > 0)
                return Vector3.One / inertia;
            else
                return Vector3.Zero;
        }

        public override float GetInverseMass()
        {
            if (mass > 0)
                return 1 / mass;
            else
                return 0;
        }

        public override Vector3 GetLinearVelocity()
        {
            return linearVelocity;
        }

        public override float GetMass()
        {
            return mass;
        }

        public override float GetPerpFactor(Vector3 force, Vector3 position)
        {
            Vector3 axis = Vector3.Normalize(Vector3.Cross(force, position));

            float perpPart = Vector3.Dot(force, (Vector3.Normalize(Vector3.Cross(position, axis))));
            return perpPart;
        }

        public override float GetPointInertia(Vector3 worldPoint)
        {
            Vector3 worldInertia = Vector3.Transform(GetInverseInertia(), transform.rotation);
            Vector3 localPoint = worldPoint - transform.position;

            float invinertia = (Vector3.Normalize(Vector3.Abs(localPoint)) * GetInverseInertia()).Length() * localPoint.Length();

            if (invinertia < float.Epsilon && invinertia > -float.Epsilon)
                return float.PositiveInfinity;

            return 1 / invinertia;
        }

        public override Vector3 GetTangentVelocity(Vector3 worldPoint, out Vector3 axis)
        {

            axis = Vector3.Normalize(angularVelocity);
            if (angularVelocity.Length() <= 0)
                axis = new Vector3(0, 1, 0);
            Vector3 point = worldPoint - transform.position;//TODO Add center of mass
            Vector3 onplane = (new Vector3(1) - Vector3.Normalize(axis)) * point;
            Vector3 tangentVelocity = new Vector3();
            if (onplane.Length() > 0)
            {
                Vector3 direction = Vector3.Transform(Vector3.Normalize(onplane), Quaternion.CreateFromAxisAngle(axis, (float)Math.PI / 2));
                tangentVelocity = direction * (onplane.Length() * 2) * (float)Math.PI * (float)(angularVelocity.Length() / (2 * Math.PI));
            }
            //We get the get the speed by multiplying the perimitre and the perimitres per second of rotation.
            return tangentVelocity;
        }
    }
}
