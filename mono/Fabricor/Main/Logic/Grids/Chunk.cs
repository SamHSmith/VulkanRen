using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using Fabricor.Main.Rendering;
using Fabricor.Main.Rendering.Loading;
using Fabricor.Main.Rendering.Models;

namespace Fabricor.Main.Logic.Grids
{
    public class Chunk
    {
        public ushort[,,] blocks = new ushort[16, 16, 16];
        public TexturedModel model { get; private set; }
        public int xCoord = 0, yCoord = 0, zCoord = 0;
        public bool ShouldUpdate = false;
        internal TypedIndex shape;
        internal BodyInertia inertia;


        public Chunk(int xCoord, int yCoord, int zCoord)
        {
            this.xCoord = xCoord;
            this.yCoord = yCoord;
            this.zCoord = zCoord;
            model = new TexturedModel(MasterRenderer.GlLoader.LoadToDynamicVAO(new float[0], new float[0], new int[0]), BlockLookup.AtlasTexture);
            
        }

        private int Value(int x, int y, int z)
        {
            if(x<0||y < 0||z < 0||x>=16||y >= 16 ||z >= 16)
            {
                return 0;
            }
            return blocks[x, y, z];
        }

        private int Value(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= 16 || y >= 16 || z >= 16)
            {
                return 0;
            }
            return blocks[x, y, z];
        }

        public void UpdateModel()
        {
            ShouldUpdate = false;
            List<float> verts = new List<float>();
            List<float> texCoords = new List<float>();
            List<int> indices = new List<int>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
<<<<<<< HEAD
                        if (Value(x - 1, y, z) > 0 && Value(x + 1, y, z) > 0 && Value(x, y - 1, z) > 0
=======
                        if (Value(x - 1, y, z) > 0&& Value(x + 1, y, z) > 0&& Value(x, y - 1, z) > 0
>>>>>>> c309e982ebb6a38a7f1240baf92ec09938fe4f60
                        && Value(x, y + 1, z) > 0 && Value(x, y, z - 1) > 0 && Value(x, y, z + 1) > 0)
                            continue;

                        if (Value(x, y, z) <= 0)
<<<<<<< HEAD
                            continue;

                        int vcount = verts.Count/3;
                        Mesh m = BlockLookup.GetBlockMesh(blocks[x,y,z]);
=======
                            continue; 

                        int vcount = verts.Count / 3;
                        Rendering.Loading.Mesh m = BlockLookup.GetBlockMesh(blocks[x, y, z]);
>>>>>>> c309e982ebb6a38a7f1240baf92ec09938fe4f60
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

            MasterRenderer.GlLoader.UpdateDynamicVAO((DynamicModel)model.RawModel, 0, verts.ToArray(), 3);
            MasterRenderer.GlLoader.UpdateDynamicVAO((DynamicModel)model.RawModel, 1, texCoords.ToArray(), 2);
            MasterRenderer.GlLoader.UpdateDynamicVAO((DynamicModel)model.RawModel, indices.ToArray());
        }

        public void UpdateShape(Simulation s)
        {
            

            if (shape.Index!=0)
                s.Shapes.Remove(shape);
            shape = LogicMaster.CubeShapeIndex;

            BufferPool pool = new BufferPool();
            CompoundBuilder b = new CompoundBuilder(pool, LogicMaster.Simulation.Shapes, blocks.Length);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {

                        if (blocks[x, y, z] > 0)
                        {
                            RigidPose pose = new RigidPose(new Vector3(x, y, z));

                            s.Shapes.GetShape<Box>(LogicMaster.CubeShapeIndex.Index).ComputeInertia(BlockLookup.GetBlockMass(blocks[x, y, z]), out var inertia);


                            b.Add(LogicMaster.CubeShapeIndex,
                                    in pose, in inertia.InverseInertiaTensor, inertia.InverseMass);
                        }

                    }
                }
            }

            b.BuildDynamicCompound(out var compoundChildren, out var compoundInertia, out var compoundCenter);
            b.Reset();

            inertia = compoundInertia;

            if (compoundChildren.Length <= 0)
                return;

            BigCompound compound = new BigCompound(compoundChildren,s.Shapes,pool);

            shape=s.Shapes.Add<BigCompound>(in compound);
        }


    }
}