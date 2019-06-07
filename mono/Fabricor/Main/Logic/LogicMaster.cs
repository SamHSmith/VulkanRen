using System;
using System.Collections.Generic;
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

        private static Grid g = new Grid();
        private static Grid g1 = new Grid();

        public static void Init()
        {
            bufferPool = new BufferPool(262144, 20);
            Simulation = Simulation.Create(bufferPool, new NarrowPhaseCallback(), new PoseIntegratorCallbacks());
            CubeShapeIndex = Simulation.Shapes.Add(new Box(0.5f, 0.5f, 0.5f));

            MasterRenderer.Init();
            updatables.Add(camera);
            updatables.Add(g);
            updatables.Add(g1);
            MasterRenderer.toRenderGrids.Add(g);

            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 15; z++)
                    {
                        g.Put(x, y, z, 1);
                        g.Put(x, z, y, 1);
                    }
                }
            }
        }

        public static void Update(float delta)
        {
            foreach (var u in updatables)
            {
                u.Update(delta);
            }

            MasterRenderer.camera = camera.transform;
            MasterRenderer.MasterRender(delta);
        }

        public static void CleanUp()
        {
            MasterRenderer.CleanUp();
        }

    }
}
