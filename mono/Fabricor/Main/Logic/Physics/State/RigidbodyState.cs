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
    }
}
