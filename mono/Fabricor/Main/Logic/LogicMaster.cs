using System;
using System.Collections.Generic;
using Fabricor.Main.Logic.Grids;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic
{
    public class LogicMaster
    {
        public static List<IUpdatable> updatables = new List<IUpdatable>();
        private static DebugCamera camera= new DebugCamera();

        private static Grid g = new Grid();

        public static void Init()
        {
            MasterRenderer.Init();
            updatables.Add(camera);
            updatables.Add(g);
            MasterRenderer.toRenderGrids.Add(g);

            for (int x = 0; x < 50; x++)
            {
                for (int z = 0; z < 50; z++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        g.Put(x, y, z, 1);
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
