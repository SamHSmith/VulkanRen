using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public struct ContactPoint
    {
        public Vector3 position;
        public Vector3 normal;
        public float depth;
        public Collidable bodyA;
        public Collidable bodyB;
    }
}
