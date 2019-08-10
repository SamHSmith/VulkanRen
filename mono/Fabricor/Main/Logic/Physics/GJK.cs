using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public static class GJK
    {
        public static float DoGJK(ISupportable a, ISupportable b, Transform at, Transform bt, out Vector3 normal)
        {
            List<Vector3> simplex = new List<Vector3>();
            simplex.Add(Support(a, b, at, bt, Vector3.One));//Pick a random point
            Vector3 dir = -simplex[0];

            //Finished initialising

            while (true)
            {
                Vector3 newpoint = Support(a, b, at, bt, dir);

                if (Vector3.Dot(newpoint, dir) < 0)//newpoint is not passed the origin
                {
                    normal = Vector3.Normalize(dir);
                    return Vector3.Dot(newpoint, dir);
                }
                simplex.Insert(0, newpoint);

                float d = DoSimplex(simplex, out simplex, out dir);

                if (d > 0)
                {
                    Console.WriteLine(d);
                    normal = Vector3.Normalize(dir);
                    return d;
                }
            }
        }

        public static Vector3 Support(ISupportable a, ISupportable b, Transform at, Transform bt, Vector3 dir)
        {
            return a.GetFurthestPointInDirection(dir, at) -
            b.GetFurthestPointInDirection(-dir, bt);
        }

        public static float DoSimplex(List<Vector3> insimplex, out List<Vector3> outsimplex, out Vector3 newdir)
        {
            if (insimplex.Count < 3)//Line
            {
                Vector3 BA = insimplex[1] - insimplex[0];


                newdir = Vector3.Cross(Vector3.Cross(BA, -insimplex[0]), BA);//BA x AO x BA

                outsimplex = insimplex;
                return -1;
            }
            else if (insimplex.Count < 4)//Triangle
            {
                Vector3 AB = insimplex[1] - insimplex[0];
                Vector3 AC = insimplex[2] - insimplex[0];
                Vector3 facenormal = Vector3.Cross(AB, AC);
                Vector3 toOrigin =-insimplex[0];
                if (Vector3.Dot(Vector3.Cross(facenormal, AC), toOrigin) > 0)
                {
                    newdir = Vector3.Cross(Vector3.Cross(AC, toOrigin), AC);
                }
                else
                {
                    Vector3 debug = Vector3.Cross(facenormal, -AB);
                    if (Vector3.Dot(Vector3.Cross(facenormal, -AB), toOrigin) > 0)
                    {
                        newdir = Vector3.Cross(Vector3.Cross(AB, toOrigin), AB);
                    }
                    else
                    {
                        if (Vector3.Dot(facenormal, toOrigin) > 0)
                        {
                            newdir = facenormal;
                        }
                        else
                        {
                            newdir = -facenormal;
                            Vector3 b = insimplex[1];
                            Vector3 c = insimplex[2];
                            insimplex[1] = c;
                            insimplex[2] = b;
                        }
                    }
                }
                outsimplex = insimplex;
                return -1;
            }
            else //Tetrahedron
            {
                Vector3 a = insimplex[0];
                Vector3 b = insimplex[1];
                Vector3 c = insimplex[2];
                Vector3 d = insimplex[3];

                Vector3 abc = Vector3.Normalize(Vector3.Cross(a - b, a - c));
                float dotabc = Vector3.Dot(abc, -insimplex[0]);

                Vector3 acd = Vector3.Normalize(Vector3.Cross(a - c, a - d));
                float dotacd = Vector3.Dot(acd, -insimplex[0]);

                Vector3 adb = Vector3.Normalize(Vector3.Cross(a - d, a - b));
                float dotadb = Vector3.Dot(adb, -insimplex[0]);

                if (dotabc > 0)
                    insimplex.RemoveAt(3);
                else if (dotacd > 0)
                    insimplex.RemoveAt(1);
                else if (dotadb > 0)
                    insimplex.RemoveAt(2);
                else
                {
                    outsimplex = insimplex;
                    newdir = Vector3.UnitY;
                    return 0.1f;
                }

                return DoSimplex(insimplex, out outsimplex, out newdir);
            }
        }
    }
}
