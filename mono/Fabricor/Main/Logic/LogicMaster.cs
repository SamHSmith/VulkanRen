using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities.Memory;
using Fabricor.Main.Logic.Grids;
using Fabricor.Main.Logic.Physics;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic
{
    public class LogicMaster
    {
        public static List<IUpdatable> updatables = new List<IUpdatable>();
        public static Simulation Simulation { get; private set; }
        private static BufferPool bufferPool;


        public static TypedIndex CubeShapeIndex { get; private set; }
        private static DebugCamera camera= new DebugCamera();

        private static Grid g;
        private static Grid g1;

        public static void Init()
        {
            bufferPool = new BufferPool(262144, 20);
            Simulation = Simulation.Create(bufferPool, new NarrowPhaseCallback(), new PoseIntegratorCallbacks(new Vector3(0,-0.1f,0)));
            CubeShapeIndex = Simulation.Shapes.Add(new Box(1f, 1f, 1f));

            MasterRenderer.Init();
            updatables.Add(camera);

            g = new Grid(true);
            g1 = new Grid();
            updatables.Add(g);
            updatables.Add(g1);
            MasterRenderer.toRenderGrids.Add(g);
            MasterRenderer.toRenderGrids.Add(g1);

            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 1; y++)
                {
                    for (int z = 0; z < 15; z++)
                    {
                        g.Put(x, y, z, 1);
                    }
                }
            }
            g1.Put(0, 0, 0, 1);
            g1.transform.position.Y = 3;
            g1.transform.position.X = 0.5f;

            //g.Put(1, 0, 2, 1);
            //g.Put(0, 1, 0, 1);
        }

        public static void Update(float delta)
        {
            foreach (var u in updatables)
            {
                u.Update(delta);
            }

            Simulation.Timestep(delta);

            MasterRenderer.camera = camera.transform;
            MasterRenderer.MasterRender(delta);
        }

        public static void CleanUp()
        {
            MasterRenderer.CleanUp();
        }

    }
}
