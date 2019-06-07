using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic.Grids
{
    public class Grid : IUpdatable
    {
        private List<Chunk> chunks = new List<Chunk>();
        public Transform transform = new Transform();
        private BodyReference body = new BodyReference();
        private TypedIndex shape;
        private BodyInertia inertia;

        public Grid()
        {
            
        }

        private void CreateBody()
        {
            BodyDescription desc = new BodyDescription();
            BepuUtilities.Quaternion rot = new BepuUtilities.Quaternion(transform.rotation.X, transform.rotation.Y, transform.rotation.Z, transform.rotation.W);
            desc.Pose = new RigidPose(in transform.position, in rot);
            desc.LocalInertia = inertia;
            if (shape.Index!=0)
                desc.Collidable = new CollidableDescription(shape, 0.02f);
            int handle = LogicMaster.Simulation.Bodies.Add(desc);
            Console.WriteLine(handle);
            body.Handle = handle;
        }

        private void UpdateShape()
        {
            if(body.Handle!=0)
                LogicMaster.Simulation.Bodies.Remove(body.Handle);
            if (shape.Index!=0)
                LogicMaster.Simulation.Shapes.Remove(shape);

            BufferPool pool = new BufferPool();
            CompoundBuilder b = new CompoundBuilder(pool,LogicMaster.Simulation.Shapes,chunks.Count);
            foreach (var ch in chunks)
            {
                RigidPose pose = new RigidPose(new Vector3(ch.xCoord * 16, ch.yCoord * 16, ch.zCoord * 16));
                b.Add(LogicMaster.CubeShapeIndex, in pose, ch.inertia.InverseInertiaTensor,ch.inertia.InverseMass);
            }


            b.BuildDynamicCompound(out var compoundChildren, out var compoundInertia, out var compoundCenter);
            b.Reset();

            inertia = compoundInertia;

            BigCompound compound = new BigCompound(compoundChildren,LogicMaster.Simulation.Shapes,pool);

            shape=LogicMaster.Simulation.Shapes.Add<BigCompound>(in compound);
            CreateBody();
        }

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
                    c.UpdateShape(LogicMaster.Simulation);
                    count++;
                }
                if (count >= Settings.Settings.ChunkGeneratePerGridPerFrame)
                    break;
            }
            if(count>0)
                UpdateShape();

            if (count == 0)
            {
                //Console.WriteLine(body.Handle);
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
