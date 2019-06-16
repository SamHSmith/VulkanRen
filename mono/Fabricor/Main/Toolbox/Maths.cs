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

        public static Vector3 Average(params Vector3[] vec)
        {
            if (vec.Length == 0)
                return new Vector3();
            Vector3 v = new Vector3();
            for (int i = 0; i < vec.Length; i++)
            {
                v += vec[i];
            }
            v /= vec.Length;
            return v;
        }

        public static Vector3 SmallestComponent(Vector3 v)
        {
            if (v.X < v.Y && v.X < v.Z)
                return new Vector3(v.X, 0, 0);

            if (v.Y < v.Z)
                return new Vector3(0, v.Y, 0);
            else
                return new Vector3(0, 0, v.Z);
        }
    }
}
