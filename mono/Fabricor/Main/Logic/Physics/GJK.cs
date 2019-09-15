using System;
using System.Collections.Generic;
using System.Numerics;

namespace Fabricor.Main.Logic.Physics
{
    public static class GJK
    {
        public const int iterationCap = 16;

        public static float DoEPA(ISupportable a, ISupportable b, Transform at, Transform bt, out Vector3 normal,
             out Vector3 position, in List<Triangle> poly, List<SupportPoint> polyAsPointList)
        {
            bool notFinished = true;
            while (notFinished)
            {
                poly.Sort((x, y) => (-x.depth).CompareTo(-y.depth));


                Triangle triangle = poly[0];
                if(poly.Count > 0)
                    poly.RemoveAt(0);
                SupportPoint newp = Support(a, b, at, bt, triangle.normal);

                Triangle atr = new Triangle(newp, triangle.b, triangle.a, false);
                Triangle btr = new Triangle(newp, triangle.c, triangle.b, false);
                Triangle ctr = new Triangle(newp, triangle.a, triangle.c, false);

                Console.WriteLine("Triangle " + triangle);
                Console.WriteLine(atr);
                Console.WriteLine(btr);
                Console.WriteLine(ctr);
                Console.WriteLine(triangle.Contains(newp.point));

                if (triangle.Contains(newp.point)||(poly.Contains(atr)&& poly.Contains(btr)&& poly.Contains(ctr)))
                {
                    //done
                    position = triangle.normal*Vector3.Dot(triangle.normal,triangle.a.point);
                    position += (triangle.a.sup_a + triangle.a.sup_b + triangle.b.sup_a + triangle.b.sup_b + triangle.c.sup_a + triangle.c.sup_b) / 6;
                    normal = triangle.normal;
                    Vector3 AtoB = at.position - bt.position;
                    if (Vector3.Dot(normal, AtoB) < 0)
                        normal *= -1;
                    return -triangle.NormalDepth();
                }
                else
                {

                    if (!poly.Contains(atr)&&atr.depth<0)
                        poly.Add(atr);
                    if (!poly.Contains(btr) && btr.depth < 0)
                        poly.Add(btr);
                    if (!poly.Contains(ctr) && ctr.depth < 0)
                        poly.Add(ctr);
                }

            }
            position = Vector3.Zero;
            normal = Vector3.UnitX;
            return 0.1f;
        }

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
                    Console.WriteLine("GJK is not doing well");
                    return -0.1f;
                }

                simplex.Insert(0, newpoint);

                float d = DoSimplex(simplex, out simplex, out dir);
                i++;


                if (d > 0)
                {
                    List<Triangle> poly = new List<Triangle>();
                    poly.Add(new Triangle(simplex[0], simplex[1], simplex[2], true));
                    poly.Add(new Triangle(simplex[0], simplex[2], simplex[3], true));
                    poly.Add(new Triangle(simplex[0], simplex[3], simplex[1], true));
                    poly.Add(new Triangle(simplex[1], simplex[2], simplex[3], true));

                    return DoEPA(a, b, at, bt, out normal, out position, poly,new List<SupportPoint>(simplex));
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
                    /*
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
                    return depth;*/
                    newdir = Vector3.UnitX;
                    outsimplex = insimplex;
                    return 1;
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

        public override string ToString()
        {
            return point.ToString();
        }
    }

    public struct Triangle
    {
        const float CompareError = 1f / 1000;

        public SupportPoint a;
        public SupportPoint b;
        public SupportPoint c;

        public Vector3 normal;
        public float depth;

        public Triangle(SupportPoint a, SupportPoint b, SupportPoint c, bool fixNormal) : this()
        {
            this.a = a;
            this.b = b;
            this.c = c;

            this.normal = Vector3.Normalize(Vector3.Cross(a.point-b.point, a.point-c.point));
            this.depth = NormalDepth();//This is to figure out if the normal is pointing the right way

            if (fixNormal&&this.depth > 0)
            {
                SupportPoint p = a;
                a = b;
                b = p;
                this.normal = Vector3.Normalize(Vector3.Cross(a.point - b.point, a.point - c.point));
                this.depth = CalculateDepth();
            }
            else
            {
                this.depth = CalculateDepth();
            }
        }

        private float CalculateDepth()
        {

                Vector3 ba = -Vector3.Normalize(Vector3.Cross(b.point - a.point, normal));
                Vector3 cb = -Vector3.Normalize(Vector3.Cross(c.point - b.point, normal));
                Vector3 ac = -Vector3.Normalize(Vector3.Cross(a.point - c.point, normal));

                float badot = Vector3.Dot(ba, -b.point);
                float cbdot = Vector3.Dot(cb, -c.point);
                float acdot = Vector3.Dot(ac, -a.point);

                if ((badot < 0 && cbdot < 0) || (badot < 0 && acdot < 0) || (cbdot < 0 && acdot < 0))
                {
                    ba *= -1;
                    cb *= -1;
                    ac *= -1;
                    badot = Vector3.Dot(ba, -b.point);
                    cbdot = Vector3.Dot(cb, -c.point);
                    acdot = Vector3.Dot(ac, -a.point);
                }

                if (badot < 0)
                    ba = -Vector3.Normalize(Vector3.Cross(b.point - a.point, Vector3.Cross(-b.point, b.point - a.point)));
                if (cbdot < 0)
                    cb = -Vector3.Normalize(Vector3.Cross(c.point - b.point, Vector3.Cross(-c.point, c.point - b.point)));
                if (acdot < 0)
                    ac = -Vector3.Normalize(Vector3.Cross(a.point - c.point, Vector3.Cross(-a.point, a.point - c.point)));

                badot = Vector3.Dot(ba, -b.point);
                cbdot = Vector3.Dot(cb, -c.point);
                acdot = Vector3.Dot(ac, -a.point);
                float normdot = NormalDepth();

                float result = normdot;

                if (badot < 0 && badot < result)
                    result = badot;
                if (cbdot < 0 && cbdot < result)
                    result = cbdot;
                if (acdot < 0 && acdot < result)
                    result = acdot;

                return result;
        }

        public float NormalDepth()
        {
            return Vector3.Dot(normal, -a.point);
        }

        public override string ToString()
        {
            return depth.ToString();
        }

        public bool Contains(Vector3 p)
        {
            float diff = Vector3.Dot(normal, p) - Vector3.Dot(normal, a.point);
            Console.WriteLine(diff);
            if (diff<=CompareError)
            {
                return true;
            }
            return false;
        }
    }
}
