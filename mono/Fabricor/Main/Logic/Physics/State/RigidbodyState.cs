using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.State
{
    public struct RigidbodyState
    {
        public Transform transform;
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;
        public Vector3 inertia;
        public float mass;
        public bool IsAssigned;
        public Vector3 massOffset;

        public Vector3 GetInertia()
        {
            return inertia;
        }

        public Vector3 GetInverseInertia()
        {
            if (inertia.Length() > 0)
                return Vector3.One / inertia;
            else
                return Vector3.Zero;
        }

        public float GetInverseMass()
        {
            if (mass > 0)
                return 1 / mass;
            else
                return 0;
        }

        public float GetMass()
        {
            return mass;
        }

        public Vector3 GetAngularVelocity()
        {
            return angularVelocity;
        }

        public Vector3 GetLinearVelocity()
        {
            return linearVelocity;
        }

        public Vector3 GetDistanceToCenterOfMass(Vector3 worldposition)
        {
            return (worldposition - (transform.position+Vector3.Transform(massOffset,transform.rotation)));
        }

        public void ApplyTorque(Vector3 torque)
        {
            Vector3 angChange = torque * GetInverseInertia();
            angularVelocity += angChange;
        }

        public void ApplyLinearForce(Vector3 force)
        {
            linearVelocity += force * GetInverseMass();
        }
    }
}
