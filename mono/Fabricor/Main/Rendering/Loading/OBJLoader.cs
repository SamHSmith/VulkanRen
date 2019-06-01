using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Fabricor.Main.Rendering.Loading
{
    public class OBJLoader
    {
        public static Mesh LoadFromOBJ(string name)
        {
            Stream s = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Fabricor.Resources.Models." + name + ".obj");
            StreamReader r = new StreamReader(s);

            List<Vector3> verticies = new List<Vector3>();
            List<Vector2> textures = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            float[] vert=null, text=null, norm=null;
            int[] ind=null;

            string line = "";
            try
            {
                while (true)
                {
                    line = r.ReadLine();
                    string[] currentline = line.Split(' ');

                    if (line.StartsWith("v ", StringComparison.Ordinal))
                    {
                        Vector3 vertex = new Vector3(float.Parse(currentline[1]), float.Parse(currentline[2]), float.Parse(currentline[3]));
                        verticies.Add(vertex);
                    }
                    else if (line.StartsWith("vt ", StringComparison.Ordinal))
                    {
                        Vector2 texture = new Vector2(float.Parse(currentline[1]), float.Parse(currentline[2]));
                        textures.Add(texture);
                    }
                    else if (line.StartsWith("vn ", StringComparison.Ordinal))
                    {
                        Vector3 normal = new Vector3(float.Parse(currentline[1]), float.Parse(currentline[2]), float.Parse(currentline[3]));
                        normals.Add(normal);
                    }
                    else if (line.StartsWith("f ", StringComparison.Ordinal))
                    {
                        break;
                    }
                }
                text = new float[verticies.Count * 2];
                norm = new float[verticies.Count * 3];

                while (line != null)
                {
                    if(!line.StartsWith("f ", StringComparison.Ordinal))
                    {
                        line = r.ReadLine();
                        continue;
                    }
                    string[] current = line.Split(' ');
                    string[] v1 = current[1].Split('/');
                    string[] v2 = current[2].Split('/');
                    string[] v3 = current[3].Split('/');

                    ProccesVertex(v1, indices, textures, normals, ref text, ref norm);
                    ProccesVertex(v2, indices, textures, normals, ref text, ref norm);
                    ProccesVertex(v3, indices, textures, normals, ref text, ref norm);
                    line = r.ReadLine();
                }
                r.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error loading model: " + name);
                Console.WriteLine(e.StackTrace);
            }

            vert = new float[verticies.Count * 3];
            ind = indices.ToArray();

            int vp = 0;
            foreach (var vec in verticies)
            {
                vert[vp] = vec.X;
                vp++;
                vert[vp] = vec.Y;
                vp++;
                vert[vp] = vec.Z;
                vp++;
            }
            Mesh m = new Mesh();
            m.vertices = vert;
            m.texCoords = text;
            m.indices = ind;
            return m;
        }

        private static void ProccesVertex(string[] vdata,List<int> indicies, List<Vector2> texture, List<Vector3> normals,ref float[] tex, ref float[] norm)
        {
            int vertexPointer = int.Parse(vdata[0])-1;
            indicies.Add(vertexPointer);
            Vector2 curtex = texture[int.Parse(vdata[1]) - 1];
            tex[vertexPointer * 2] = curtex.X;
            tex[vertexPointer * 2 + 1] = 1 - curtex.Y;
            Vector3 currentNorm = normals[int.Parse(vdata[2]) - 1];
            norm[vertexPointer * 3] = currentNorm.X;
            norm[vertexPointer * 3+1] = currentNorm.Y;
            norm[vertexPointer * 3+2] = currentNorm.Z;
        }
    }

    public class Mesh
    {
        public float[] vertices;
        public float[] texCoords;
        public int[] indices;
    }
}
