using System;
using System.Collections.Generic;
using Fabricor.Main.Rendering;

namespace Fabricor.Main.Logic
{
    public class LogicMaster
    {
        public static List<IUpdatable> updatables = new List<IUpdatable>();
        private static DebugCamera camera= new DebugCamera();

        public static void Init()
        {
            MasterRenderer.Init();
            updatables.Add(camera);
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
