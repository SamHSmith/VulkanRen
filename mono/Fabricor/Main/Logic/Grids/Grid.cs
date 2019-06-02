using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic.Grids
{
    public class Grid : IUpdatable
    {
        private List<Chunk> chunks = new List<Chunk>();
        public Transform transform = new Transform();

        public List<RenderObject> GetRenderObjects()
        {
            List<RenderObject> objs = new List<RenderObject>();
            foreach (var c in chunks)
            {
                RenderObject o = new RenderObject(transform.LocalToWorldSpace(new Transform(new Vector3(c.xCoord*16,c.yCoord*16,c.zCoord*16))),c.model);
                objs.Add(o);
            }
            return objs;
        }

        public void Update(float delta)
        {
            int count = 0;
            foreach (var c in chunks)
            {
                if (c.ShouldUpdate)
                {
                    c.UpdateModel();
                    count++;
                }
                if (count >= Settings.Settings.ChunkGeneratePerGridPerFrame)
                    return;
            }
        }

        public void Put(int x, int y, int z, ushort block)
        {
            int cx = x - (x % 16);
            int cy = y - (y % 16);
            int cz = z - (z % 16);

            Chunk c = GetChunk(cx / 16, cy / 16, cz / 16);
            c.blocks[x - cx, y - cy, z - cz]=block;
            c.ShouldUpdate = true;
        }

        public ushort Get(int x, int y, int z)
        {
            int cx = x - (x % 16);
            int cy = y - (y % 16);
            int cz = z - (z % 16);

            Chunk c = GetChunk(cx / 16, cy / 16, cz / 16);
            return c.blocks[x - cx, y - cy, z - cz];
        }


        private Chunk GetChunk(int x, int y, int z)
        {
            Chunk c = null;
            foreach (var ch in chunks)
            {
                if(ch.xCoord==x&& ch.yCoord == y && ch.zCoord == z)
                {
                    c = ch;
                }
            }
            if (c == null)
            {
                c = new Chunk(x,y,z);
                chunks.Add(c);
            }
            return c;
        }
    }
}
