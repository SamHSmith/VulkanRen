using System;
using System.Collections.Generic;
using Fabricor.Main.Rendering;
using Fabricor.Main.Rendering.Loading;
using Fabricor.Main.Rendering.Models;

namespace Fabricor.Main.Logic.Grids
{
    public class Chunk
    {
        public uint[,,] blocks=new uint[16,16,16];
        public DynamicModel model { get; private set; }
        public int xCoord = 0, yCoord = 0, zCoord = 0;

        public Chunk()
        {
            model = MasterRenderer.GlLoader.LoadToDynamicVAO(new float[0], new float[0], new int[0]);
        }

        public void UpdateModel()
        {
            List<float> verts = new List<float>();
            List<float> texCoords = new List<float>();
            List<int> indices = new List<int>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (blocks[x, y, z] <= 0)
                        {
                            continue;
                        }
                        int vcount = verts.Count/3;
                        Mesh m = BlockLookup.GetBlockMesh(blocks[x,y,z]);
                        for (int i = 0; i < m.vertices.Length; i++)
                        {
                            verts.Add(m.vertices[i] + x);
                            i++;
                            verts.Add(m.vertices[i] + y);
                            i++;
                            verts.Add(m.vertices[i] + z);
                        }
                        for (int i = 0; i < m.texCoords.Length; i++)
                        {
                            texCoords.Add(m.texCoords[i]);
                            i++;
                            texCoords.Add(m.texCoords[i]);
                        }
                        for (int i = 0; i < m.indices.Length; i++)
                        {
                            indices.Add(m.indices[i] + vcount);
                        }
                    }
                }
            }

            MasterRenderer.GlLoader.UpdateDynamicVAO(model, 0, verts.ToArray(), 3);
            MasterRenderer.GlLoader.UpdateDynamicVAO(model, 1, texCoords.ToArray(), 2);
            MasterRenderer.GlLoader.UpdateDynamicVAO(model, indices.ToArray());
        }

        
    }
}
