using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public struct ContactPoint
    {
        public Vector3 position;
        public Vector3 normal;
        public float depth;
        public IShapeRoot bodyA;
        public IShapeRoot bodyB;
    }
}
