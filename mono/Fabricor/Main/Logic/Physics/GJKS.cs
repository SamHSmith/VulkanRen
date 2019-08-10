using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Logic.Physics.Shapes;

namespace Fabricor.Main.Logic.Physics
{
    public static class GJKS
    {
        public static ContactPoint GetPenetration(Transform at, Transform bt, ISupportable a, ISupportable b)
        {
            Vector3 CO = -GetMinkowskiCenter(at, bt, a, b);

            List<Vector3> past = new List<Vector3>();
            past.Add(Support(CO, at, bt, a, b));
            past.Add(Support(Vector3.UnitX * CO.X, at, bt, a, b));
            past.Add(Support(Vector3.UnitY * CO.Y, at, bt, a, b));
            past.Add(Support(Vector3.UnitZ * CO.Z, at, bt, a, b));

            float depth = float.MaxValue;
            Vector3 normal = Vector3.Zero;
            Vector3 position= Vector3.Zero;

            foreach (var dir in past)
            {
                Vector3 p = Support(-dir, at, bt, a, b);
                Vector3 normDir = Vector3.Normalize(dir);
                float d = Vector3.Dot(-normDir, p);

                if (d < depth)
                {
                    depth = d;
                    normal = -normDir;
                    position = p;
                }

            }

            Console.WriteLine(normal + " " + depth+" at "+position);

            if (depth > 0)
                return new ContactPoint { normal = -normal, depth = depth, position = new Vector3[] {position } };

            return new ContactPoint { normal = Vector3.Zero };
        }

        private static Vector3 GetMinkowskiCenter(Transform at, Transform bt, ISupportable a, ISupportable b)
        {
            Vector3 av = a.GetCenter(at);
            Vector3 bv = b.GetCenter(bt);
            return av - bv;
        }

        private static Vector3 Support(Vector3 dir, Transform at, Transform bt, ISupportable a, ISupportable b)
        {
            return a.Support(at, dir) - b.Support(bt, -dir);
        }
    }
}
