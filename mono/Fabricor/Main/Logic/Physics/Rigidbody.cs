using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public class Rigidbody : Collidable
    {

        public Vector3 linearVelocity;
        public Vector3 angularVelocity;
        public float mass=1;
        public Vector3 inverseInertia = Vector3.One;

        public Rigidbody()
        {
            transform = new Transform(new Vector3());
        }

        public override void ApplyLocalForce(Vector3 position, Vector3 force)
        {
            //TODO add center of mass
            Console.WriteLine("Force " + force);



            Vector3 axis = Vector3.Normalize(Vector3.Cross(force,position));
            Console.WriteLine("Axis " + axis);
            Console.WriteLine("Force Position" + force+" "+position);

            float invinertia = (Vector3.Abs(axis)*inverseInertia).Length();

            Console.WriteLine("inertia " + invinertia);

            float perpPart = Vector3.Dot(force, Vector3.Abs(Vector3.Normalize(Vector3.Cross(position, axis))));

            float speedChange = invinertia * force.Length()*perpPart;

            angularVelocity += axis * speedChange / position.Length() * 2 * (float)Math.PI;

            Console.WriteLine("perp " + perpPart);
            Console.WriteLine("spdChange " + speedChange);

            linearVelocity += force / GetMass();
        }

        public override float GetMass()
        {
            return mass;
        }

        public override Vector3 GetPointVelocity(Vector3 worldPoint)
        {
            Vector3 axis = Vector3.Normalize(angularVelocity);
            if (angularVelocity.Length() <= 0)
                axis = new Vector3(0, 1, 0);
            Vector3 point = worldPoint-transform.position;//TODO Add center of mass
            Vector3 onplane = (new Vector3(1) - Vector3.Normalize(axis)) * point;
            Vector3 tangentVelocity = new Vector3();
            if (onplane.Length() > 0)
            {
                Vector3 direction = Vector3.Transform(Vector3.Normalize(onplane), Quaternion.CreateFromAxisAngle(axis, (float)Math.PI / 2));
                tangentVelocity = direction * (onplane.Length() * 2) * (float)Math.PI * (float)(angularVelocity.Length() / (2 * Math.PI));
            }
            //We get the get the speed by multiplying the perimitre and the perimitres per second of rotation.
            return tangentVelocity + linearVelocity;
        }
    }
}
