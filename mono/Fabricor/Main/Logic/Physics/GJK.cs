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

                if(DoSimplex(simplex,out simplex,out dir))
                {
                    normal = Vector3.Normalize(dir);
                    return dir.Length();
                }
            }
        }

        public static Vector3 Support(ISupportable a, ISupportable b, Vector3 dir)
        {
            return a.GetFurthestPointInDirection(dir) - b.GetFurthestPointInDirection(-dir);
        }

        public static bool DoSimplex(List<Vector3> insimplex, out List<Vector3> outsimplex, out Vector3 newdir)
        {
            if (insimplex.Count < 3)//Line
            {
                Vector3 BA = insimplex[1] - insimplex[0];


                newdir = Vector3.Cross(Vector3.Cross(BA, -insimplex[0]), BA);//BA x AO x BA

                outsimplex = insimplex;
                return false;
            }
            else if (insimplex.Count < 4)//Triangle
            {
                Vector3 AB = insimplex[0] - insimplex[1];
                Vector3 AC = insimplex[0] - insimplex[2];
                Vector3 facenormal = Vector3.Cross(AB, AC);

                if (Vector3.Dot(Vector3.Cross(facenormal, AC), -insimplex[0]) > 0)
                {
                    if (Vector3.Dot(AC, -insimplex[0]) > 0)
                    {
                        newdir = Vector3.Cross(Vector3.Cross(AC, -insimplex[0]), AC);
                    }
                    else
                    {
                        if (Vector3.Dot(AB, -insimplex[0]) > 0)//Star
                        {
                            newdir = Vector3.Cross(Vector3.Cross(AB, -insimplex[0]), AB);
                        }
                        else
                        {
                            newdir = -insimplex[0];
                        }
                    }
                }
                else
                {
                    if (Vector3.Dot(Vector3.Cross(facenormal, AB), -insimplex[0]) > 0)
                    {
                        if (Vector3.Dot(AB, -insimplex[0]) > 0)//Star
                        {
                            newdir = Vector3.Cross(Vector3.Cross(AB, -insimplex[0]), AB);
                        }
                        else
                        {
                            newdir = -insimplex[0];
                        }
                    }
                    else
                    {
                        if (Vector3.Dot(facenormal, -insimplex[0]) > 0)
                        {
                            newdir = facenormal;
                        }
                        else
                        {
                            newdir = -facenormal;
                        }
                    }
                }
                outsimplex = insimplex;
                return false;
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

                Vector3 adb =Vector3.Normalize( Vector3.Cross(a - d, a - b));
                float dotadb = Vector3.Dot(adb, -insimplex[0]);

                if (dotabc > 0)
                {
                    newdir = abc;
                    insimplex.RemoveAt(3);
                }else if (dotacd>0)
                {
                    newdir = acd;
                    insimplex.RemoveAt(1);
                }
                else if (dotadb > 0)
                {
                    newdir = adb;
                    insimplex.RemoveAt(2);
                }
                else//inside
                {
                    if (dotabc > dotacd)
                    {
                        if (dotabc > dotadb)
                        {
                            newdir = abc * -dotabc;
                        }
                        else
                        {
                            newdir =adb * -dotadb;
                        }
                    }
                    else
                    {
                        if (dotacd > dotadb)
                        {
                            newdir = acd * -dotacd;
                        }
                        else
                        {
                            newdir = adb * -dotadb;
                        }
                    }
                    outsimplex = insimplex;
                    return true;
                }
                outsimplex = insimplex;
                return false;
            }
        }
    }
}
