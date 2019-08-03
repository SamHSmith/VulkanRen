using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public static class GJK
    {
        public static float DoGJK(ISupportable a, ISupportable b, out Vector3 normal)
        {
            List<Vector3> simplex = new List<Vector3>();
            simplex.Add(Support(a, b, Vector3.UnitY));//Pick a random point
            Vector3 dir = -simplex[0];

            //Finished initialising

            while (true)
            {
                Vector3 newpoint = Support(a, b, dir);

                if (Vector3.Dot(newpoint, dir) < 0)//newpoint is not passed the origin
                {
                    normal = Vector3.Normalize(-newpoint);
                    return -newpoint.Length();
                }
                simplex.Insert(0, newpoint);

            }
        }

        public static Vector3 Support(ISupportable a, ISupportable b,Vector3 dir)
        {
            return a.GetFurthestPointInDirection(dir) - b.GetFurthestPointInDirection(-dir);
        }

        public static bool DoSimplex(List<Vector3> insimplex,out List<Vector3> outsimplex,out Vector3 newdir)
        {
            if (insimplex.Count < 3)//Line
            {
                Vector3 BA = insimplex[1] - insimplex[0];
                if (Vector3.Dot(BA, -insimplex[0]) > 0)//BA * AO >0 
                {
                    newdir = -insimplex[0];
                }
                else
                {
                    newdir = Vector3.Cross(Vector3.Cross(BA, -insimplex[0]), BA);//BA x AO x BA
                }
                outsimplex = insimplex;
                return false;
            }
        }
    }
}
