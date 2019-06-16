using System;
using System.Collections.Generic;
using System.Numerics;
using Fabricor.Main.Logic.Grids;
using Fabricor.Main.Logic.Physics;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic
{
    public class LogicMaster
    {
        public static List<IUpdatable> updatables = new List<IUpdatable>();
        private static DebugCamera camera= new DebugCamera();

        private static Grid g = new Grid();
        private static Grid g1 = new Grid();
        private static Rigidbody rb = new Rigidbody(), rb1 = new Rigidbody();

        public static void Init()
        {
            MasterRenderer.Init();
            updatables.Add(camera);
            updatables.Add(g);
            MasterRenderer.toRenderGrids.Add(g);
            updatables.Add(g1);
            MasterRenderer.toRenderGrids.Add(g1);
            rb1.transform.position.Y = 4;
            rb1.transform.position.Z = 0.4f;
            rb1.linearVelocity.Y = -0.5f;
            rb1.angularVelocity.X = -1f;
            Simulation.rigidbodies.Add(rb);
            Simulation.rigidbodies.Add(rb1);
            rb.AddShape(new AABB {radii=new Vector3(0.5f) });
            rb1.AddShape(new AABB { radii = new Vector3(0.5f) });


            /*
            for (int x = 0; x < 50; x++)
            {
                for (int z = 0; z < 50; z++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        g.Put(x, y, z, 1);
                    }
                }
            }*/
            g.Put(0, 0, 0, 1);
            g1.Put(0, 0, 0, 1);

        }

        public static void Update(float delta)
        {
            foreach (var u in updatables)
            {
                u.Update(delta);
            }
            Simulation.TimeStep(delta);

            g.transform = rb.transform;
            g1.transform = rb1.transform;
            MasterRenderer.camera = camera.transform;
            MasterRenderer.MasterRender(delta);
        }

        public static void CleanUp()
        {
            MasterRenderer.CleanUp();
        }

    }
}
