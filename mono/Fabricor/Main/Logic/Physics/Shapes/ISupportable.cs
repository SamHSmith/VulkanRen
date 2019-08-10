using System;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics.Shapes
{
    public interface ISupportable
    {
        Vector3 Support(Transform t, Vector3 dir);
        Vector3 GetCenter(Transform t);
    }
}
