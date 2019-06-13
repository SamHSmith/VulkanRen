using System;
namespace Fabricor.Main.Logic.Physics.Shapes
{
    public interface IShape
    {
        bool IsColliding(Transform at, Transform bt, IShape other);
        bool HasImplementation(IShape s);

        AABB ToAABB();
        BoundSphere ToBoundSphere();
    }
}
