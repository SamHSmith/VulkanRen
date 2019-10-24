using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace Fabricor.Main.Logic.Physics
{
    public class EPA
    {
        public ISupportable a, b;
        public Transform at, bt;
        public List<Triangle> poly;

        private bool shouldFail = false;
        const int trianglesAddedAbort = 500;


        public EPA(ISupportable a, ISupportable b, Transform at, Transform bt, List<Triangle> poly)
        {
            this.a = a;
            this.b = b;
            this.at = at;
            this.bt = bt;
            this.poly = poly;
        }

        public float GetDepth(out Vector3 normal, out Vector3 position)
        {
            int iterations = 0;
            while (true)
            {
                iterations++;
                if (iterations > GJK.iterationCap)
                    shouldFail = true;

                if (shouldFail)
                {
                    normal = new Vector3(float.NaN);
                    position = new Vector3(float.NaN);
                    return -1;
                }
                poly.Sort((x, y) => (x.depth).CompareTo(y.depth));

                Triangle triangle = poly[0];
                SupportPoint newp = Support(triangle.normal);

                if (triangle.Behind(newp))
                {
                    CreateAndFillHole(newp);
                }
                else
                {
                    //done
                    position = triangle.normal * triangle.depth;
                    float a = (position - triangle.a.point).Length();
                    float b = (position - triangle.b.point).Length();
                    float c = (position - triangle.c.point).Length();

                    float total = a + b + c;
                    a /= total;
                    b /= total;
                    c /= total;
                    position = (a * triangle.a.AverageSupport) + (b * triangle.b.AverageSupport) + (c * triangle.c.AverageSupport);

                    normal = -triangle.normal;
                    Vector3 AtoB = at.position - bt.position;
                    if (Vector3.Dot(normal, AtoB) < 0)
                        normal *= -1;
                    return triangle.depth;
                }
            }
        }

        private void CreateAndFillHole(SupportPoint newp)
        {
            List<Triangle> tris = GetBehinds(newp);
            int removed = tris.Count;
            List<Line> lines = new List<Line>();
            foreach (var t in tris)
            {
                poly.Remove(t);
                lines.AddRange(t.ToLines());
            }

            lines = LinesInTriangles(lines, poly);
            int added = lines.Count;
            AddNewTrianglesFromLines(lines, newp);
            Console.WriteLine(added - removed);
            if (added - removed > trianglesAddedAbort)
            {
                shouldFail = true;
            }
        }

        private void AddNewTrianglesFromLines(List<Line> lines, SupportPoint newp)
        {
            foreach (var l in lines)
            {
                poly.Add(new Triangle(newp, l.a, l.b, true));
            }
        }

        private List<Line> LinesInTriangles(List<Line> lines, List<Triangle> triangles)
        {
            List<Line> newLines = new List<Line>();
            foreach (var l in lines)
            {
                foreach (var t in triangles)
                {
                    if (t.ContainsLine(l))
                        newLines.Add(l);
                }
            }
            return newLines;
        }

        private List<Triangle> GetBehinds(SupportPoint p)
        {
            List<Triangle> tris = new List<Triangle>();

            tris.AddRange(poly.Where((arg) => arg.Behind(p)));

            return tris;
        }

        private SupportPoint Support(Vector3 dir)
        {
            return GJK.Support(a, b, at, bt, dir);
        }
    }

    public struct Line
    {
        public SupportPoint a;
        public SupportPoint b;

        public Line(SupportPoint a, SupportPoint b)
        {
            this.a = a;
            this.b = b;
        }

        public bool StartsWith(SupportPoint p) => p.point == a.point;
    }
}
