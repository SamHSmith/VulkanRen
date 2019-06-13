using System;
using System.Numerics;

namespace Fabricor.Main.Toolbox
{
    public class Maths
    {
        public static float Clamp(float value,float min, float max)
        {
            if (value < min)
                value = min;

            if (value > max)
                value = max;
            return value;
        }

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            value.X = Clamp(value.X, min.X, max.X);
            value.Y = Clamp(value.Y, min.Y, max.Y);
            value.Z = Clamp(value.Z, min.Z, max.Z);

            return value;
        }
    }
}
