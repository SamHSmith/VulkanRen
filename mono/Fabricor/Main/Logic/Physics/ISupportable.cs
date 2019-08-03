using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public interface ISupportable
    {
        Vector3 GetFurthestPointInDirection(Vector3 dir);
    }
}
