using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public interface IShape
    {
        ContactPoint[] IsColliding(Transform at, Transform bt, IShape other);
        bool HasImplementation(IShape s);

        AABB ToAABB();
        BoundSphere ToBoundSphere();

        Collidable Collidable { get; set; }
    }
}
