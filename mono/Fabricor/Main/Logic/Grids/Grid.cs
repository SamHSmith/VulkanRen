using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic.Grids
{
    public class Grid : IUpdatable
    {
        private List<Chunk> chunks = new List<Chunk>();
<<<<<<< HEAD
        public Transform transform = new Transform(new Vector3());
=======
        public Transform transform = new Transform();
        private BodyReference body = new BodyReference(0,LogicMaster.Simulation.Bodies);
        private bool noBody = true;
        private TypedIndex shape;
        private BodyInertia inertia;
        public bool IsStatic { get; private set; }

        public Grid()
        {
            
        }

        public Grid(bool isStatic)
        {
            IsStatic = isStatic;
        }

        private void CreateBody()
        {
            noBody = false;

            BodyDescription desc = BodyDescription.CreateDynamic(transform.ToRigidPose(), inertia, new CollidableDescription(shape, 0.0f),
                new BodyActivityDescription(0.0f));

            if (IsStatic)
            {
                desc.LocalInertia.InverseMass = 0;
                desc.LocalInertia.InverseInertiaTensor = new Symmetric3x3();
            }
            int handle = LogicMaster.Simulation.Bodies.Add(desc);
            body.Handle = handle;
        }

        private void UpdateShape()
        {
            if (body.Handle != 0)
                LogicMaster.Simulation.Bodies.Remove(body.Handle);
            if (shape.Index != 0)
                LogicMaster.Simulation.Shapes.Remove(shape);

            BufferPool pool = new BufferPool();
            CompoundBuilder b = new CompoundBuilder(pool, LogicMaster.Simulation.Shapes, chunks.Count);
            foreach (var ch in chunks)
            {
                RigidPose pose = new RigidPose(new Vector3(ch.xCoord * 16, ch.yCoord * 16, ch.zCoord * 16));
                

                    b.Add(ch.shape, in pose, ch.inertia.InverseInertiaTensor, ch.inertia.InverseMass);

            }


            b.BuildDynamicCompound(out var compoundChildren, out var compoundInertia, out var compoundCenter);
            b.Reset();

            inertia = compoundInertia;
            Console.WriteLine("MASS: " + inertia.InverseMass);

            if (compoundChildren.Length <= 0)
                return;

            BigCompound compound = new BigCompound(compoundChildren, LogicMaster.Simulation.Shapes, pool);

            shape = LogicMaster.Simulation.Shapes.Add<BigCompound>(in compound);
            CreateBody();
        }
>>>>>>> c309e982ebb6a38a7f1240baf92ec09938fe4f60

        public List<RenderObject> GetRenderObjects()
        {
            List<RenderObject> objs = new List<RenderObject>();
            foreach (var c in chunks)
            {
                RenderObject o = new RenderObject(transform.LocalToWorldSpace(new Transform(new Vector3(c.xCoord * 16, c.yCoord * 16, c.zCoord * 16))), c.model);
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
            if (count > 0)
                UpdateShape();

            if (!noBody)
            {
                transform.FromRigidPose(body.Pose);
                
            }

        }

        public void Put(int x, int y, int z, ushort block)
        {
            int cx = x - (x % 16);
            int cy = y - (y % 16);
            int cz = z - (z % 16);

            Chunk c = GetChunk(cx / 16, cy / 16, cz / 16);
            c.blocks[x - cx, y - cy, z - cz] = block;
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
                if (ch.xCoord == x && ch.yCoord == y && ch.zCoord == z)
                {
                    c = ch;
                }
            }
            if (c == null)
            {
                c = new Chunk(x, y, z);
                chunks.Add(c);
            }
            return c;
        }
    }
}
