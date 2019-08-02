using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Fabricor.Main.Logic.Grids;
using Fabricor.Main.Logic.Physics;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Logic.Physics.State;
using Fabricor.Main.Rendering;
using Fabricor.Main.Toolbox;
using Plane = Fabricor.Main.Logic.Physics.Shapes.Plane;

namespace Fabricor.Main.Logic
{
    public class LogicMaster
    {
        public static List<IUpdatable> updatables = new List<IUpdatable>();
        private static DebugCamera camera = new DebugCamera();

        private static Thread fixedthread;
        private static bool shutdown = false;
        private static float Time = 1;
        private static float fixedDelta = 1f / 10;
        private static int updateRate = (int)TimeSpan.FromSeconds(fixedDelta).Ticks;


        private static List<Grid> gs = new List<Grid>();


        public static void Init()
        {
            Console.WriteLine("INIT");
            MasterRenderer.Init();
            updatables.Add(camera);



            Random r = new Random();
            for (int i = 0; i < 100; i++)
            {
                ConvexShape cube = new ConvexShape(new Vector3[] {
                new Vector3(-0.5f,0.5f,0.5f),
                new Vector3(-0.5f,-0.5f,0.5f),
                new Vector3(-0.5f,-0.5f,-0.5f),
                new Vector3(-0.5f,0.5f,-0.5f),
                new Vector3(0.5f,0.5f,0.5f),
                new Vector3(0.5f,-0.5f,0.5f),
                new Vector3(0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,0.5f,-0.5f), }, new Vector3[] {
                Vector3.UnitX,
                -Vector3.UnitX,
                Vector3.UnitY,
                -Vector3.UnitY,
                Vector3.UnitZ,
                -Vector3.UnitZ});

                Grid g = new Grid();
                g.Put(0, 0, 0, 1);
                g.rb = Simulation.GetNewRigidbody();
                g.rb.AddShape(cube);
                g.rb.state[0].transform.position = new Vector3((float)r.NextDouble() * 20, (float)r.NextDouble() * 20, (float)r.NextDouble() * 20);
                g.rb.state[0].linearVelocity = new Vector3((float)r.NextDouble() * 1, (float)r.NextDouble() * 1, (float)r.NextDouble() * 1);
                g.transform = g.rb.state[0].transform;
                updatables.Add(g);
                MasterRenderer.toRenderGrids.Add(g);
                gs.Add(g);
            }

            fixedthread = new Thread(FixedUpdate);
            fixedthread.Start();
        }

        private static long lastUpdate;
        public static void FixedUpdate()
        {
            lastUpdate = DateTime.UtcNow.Ticks;
            while (!shutdown)
            {
                long currentTime = DateTime.UtcNow.Ticks;
                int timePassed = (int)(currentTime - lastUpdate);
                float updates = (((float)timePassed) / updateRate) * Time;

                foreach (var g in gs)
                {
                    g.rb.state[0].linearVelocity += -Vector3.Normalize(g.rb.state[0].transform.position) * fixedDelta;
                }

                Simulation.TimeStep(fixedDelta);

                while (updates < 1)
                {
                    currentTime = DateTime.UtcNow.Ticks;
                    timePassed = (int)(currentTime - lastUpdate);
                    updates = (((float)timePassed) / updateRate) * Time;
                }
                Simulation.SwapBuffers();
                lastUpdate = DateTime.UtcNow.Ticks;
            }
        }


        public static void Update(float delta)
        {
            long currentTime = DateTime.UtcNow.Ticks;
            int timePassed = (int)(currentTime - lastUpdate);
            float updates = (((float)timePassed) / updateRate) * Time;

            Simulation.UpdateInterpolation(Maths.Clamp(updates, 0, 1), fixedDelta);

            foreach (var u in updatables)
            {
                u.Update(delta);
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.E))
            {
                Time = 0.1f;
            }
            else
            {
                Time = 1;
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.U))
            {
                Time *= 10;
            }

            foreach (var g in gs)
            {
                g.transform = g.rb.interpolatedState[0].transform;
            }


            MasterRenderer.camera = camera.transform;
            MasterRenderer.MasterRender(delta);
        }

        public static void CleanUp()
        {
            shutdown = true;
            fixedthread.Join();
            MasterRenderer.CleanUp();
            Simulation.CleanUp();
        }

    }
}
