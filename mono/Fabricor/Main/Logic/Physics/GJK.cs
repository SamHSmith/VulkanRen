using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public static class GJK
    {
        public const int iterationCap = 16;

        public static float DoGJK(ISupportable a, ISupportable b, Transform at, Transform bt, out Vector3 normal,out Vector3 position)
        {
            List<SupportPoint> simplex = new List<SupportPoint>();
            simplex.Add(Support(a, b, at, bt, Vector3.One));//Pick a random point
            Vector3 dir = -simplex[0].point;

            //Finished initialising
            int i = 0;
            while (true)
            {
                SupportPoint newpoint = Support(a, b, at, bt, dir);

                if (Vector3.Dot(newpoint.point, dir) < 0)//newpoint is not passed the origin
                {
                    normal = Vector3.Normalize(dir);
                    position = Vector3.Zero;
                    return Vector3.Dot(newpoint.point, normal);
                }
                if (i > iterationCap)
                {
                    normal = Vector3.Zero;
                    position = Vector3.Zero;
                    return -0.1f;
                }

                simplex.Insert(0, newpoint);

                float d = DoSimplex(simplex, out simplex, out dir);
                i++;


                if (d > 0)
                {
                    normal = Vector3.Normalize(dir);
                    if (Vector3.Dot(normal, at.position - bt.position) < 0)
                        normal *= -1;
                    position = simplex[0].point;
                    return d;
                }
            }
        }

        public static SupportPoint Support(ISupportable a, ISupportable b, Transform at, Transform bt, Vector3 dir)
        {
            Vector3 supa = a.GetFurthestPointInDirection(dir, at);
            Vector3 supb =b.GetFurthestPointInDirection(-dir, bt);
            return new SupportPoint { point = supa - supb, sup_a = supa, sup_b = supb };
        }

        public static float DoSimplex(List<SupportPoint> insimplex, out List<SupportPoint> outsimplex, out Vector3 newdir)
        {
            if (insimplex.Count < 3)//Line
            {
                Vector3 BA = insimplex[1].point - insimplex[0].point;


                newdir = Vector3.Cross(Vector3.Cross(BA, -insimplex[0].point), BA);//BA x AO x BA

                outsimplex = insimplex;
                return -1;
            }
            else if (insimplex.Count < 4)//Triangle
            {
                Vector3 AB = insimplex[1].point - insimplex[0].point;
                Vector3 AC = insimplex[2].point - insimplex[0].point;
                Vector3 facenormal = Vector3.Cross(AB, AC);
                Vector3 toOrigin =-insimplex[0].point;
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
                            SupportPoint b = insimplex[1];
                            SupportPoint c = insimplex[2];
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
                Vector3 a = insimplex[0].point;
                Vector3 b = insimplex[1].point;
                Vector3 c = insimplex[2].point;
                Vector3 d = insimplex[3].point;

                Vector3 abc = Vector3.Normalize(Vector3.Cross(a - b, a - c));
                float dotabc = Vector3.Dot(abc, -insimplex[0].point);

                Vector3 acd = Vector3.Normalize(Vector3.Cross(a - c, a - d));
                float dotacd = Vector3.Dot(acd, -insimplex[0].point);

                Vector3 adb = Vector3.Normalize(Vector3.Cross(a - d, a - b));
                float dotadb = Vector3.Dot(adb, -insimplex[0].point);

                if (dotabc > 0)
                    insimplex.RemoveAt(3);
                else if (dotacd > 0)
                    insimplex.RemoveAt(1);
                else if (dotadb > 0)
                    insimplex.RemoveAt(2);
                else
                {
                    float depth = 0;
                    Vector3 normal = Vector3.Zero;
                    if (dotabc > dotacd && dotabc > dotadb)
                    {
                        depth = -dotabc;
                        normal = abc;
                        insimplex.RemoveAt(3);
                    }
                    else if (dotacd > dotadb)
                    {
                        depth = -dotacd;
                        normal = acd;
                        insimplex.RemoveAt(1);
                    }
                    else
                    {
                        depth = -dotadb;
                        normal = adb;
                        insimplex.RemoveAt(2);
                    }

                    Vector3 minkpoint = normal * depth;

                    float af = (minkpoint - insimplex[0].point).Length();
                    float bf = (minkpoint - insimplex[1].point).Length();
                    float cf = (minkpoint - insimplex[2].point).Length();
                    float total = af + bf + cf;
                    af /= total;
                    bf /= total;
                    cf /= total;

                    Vector3 bodyaPoint = (insimplex[0].sup_a * af) + (insimplex[1].sup_a * bf) + (insimplex[2].sup_a * cf);
                    Vector3 bodybPoint = (insimplex[0].sup_b * af) + (insimplex[1].sup_b * bf) + (insimplex[2].sup_b * cf);

                    Vector3 ContactPoint = (bodyaPoint + bodybPoint) / 2;


                    newdir = normal;
                    outsimplex = new List<SupportPoint>();
                    outsimplex.Add(new SupportPoint { point = ContactPoint });
                    return depth;
                }

                return DoSimplex(insimplex, out outsimplex, out newdir);
            }
        }
    }

    public struct SupportPoint
    {
        public Vector3 point;
        public Vector3 sup_a;
        public Vector3 sup_b;
    }
}
