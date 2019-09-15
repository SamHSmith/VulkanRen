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
        private static float fixedDelta = 1f / 1000;
        private static int updateRate = (int)TimeSpan.FromSeconds(fixedDelta).Ticks;

        private static List<Grid> gs = new List<Grid>();


        public static void Init()
        {
            Console.WriteLine("INIT");
            MasterRenderer.Init();
            updatables.Add(camera);

            Random r = new Random(1);
            for (int i = 0; i < 2; i++)
            {


                Grid g = new Grid(Simulation.GetNewRigidbody());
                //for (int x = 0; x < 4; x++)
                {
                    //for (int y = 0; y < 2; y++)
                    {
                        //for (int z = 0; z < 2; z++)
                        {
                            g.Put(2, 0, 0, 1);
                        }
                    }
                }
                g.rb.state[0].mass = 10;
                g.rb.state[0].inertia = Vector3.One * 20;
                g.rb.state[0].transform.position =
                new Vector3(((float)r.NextDouble() - 0.5f) * 0, ((float)r.NextDouble() - 0.5f) * 10, ((float)r.NextDouble() - 0.5f) * 0);
                //g.rb.state[0].linearVelocity = new Vector3((float)r.NextDouble() * 0, (float)r.NextDouble() * 0, (float)r.NextDouble() * 0);
                g.rb.state[0].angularVelocity= new Vector3((float)r.NextDouble() * 1, (float)r.NextDouble() * 1, (float)r.NextDouble() * 1);
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
            Simulation.SwapBuffers();
            lastUpdate = DateTime.UtcNow.Ticks;
            while (!shutdown)
            {
                long currentTime = DateTime.UtcNow.Ticks;
                int timePassed = (int)(currentTime - lastUpdate);
                float updates = (((float)timePassed) / (updateRate / Time));

                foreach (var g in gs)
                {
                    g.rb.state[0].linearVelocity += -Vector3.Normalize(g.rb.state[0].transform.position)*0.2f * fixedDelta;
                }

                for (int i = 0; i < gs.Count; i++)
                {
                    for (int k = i+1; k < gs.Count; k++)
                    {
                        
                        Vector3 force = 
                        (gs[k].rb.state[0].transform.position-gs[i].rb.state[0].transform.position) * 0.02f * fixedDelta;

                        if (float.IsNaN(force.Length()))
                            continue;

                        gs[k].rb.state[0].linearVelocity -= force;
                        gs[i].rb.state[0].linearVelocity += force;
                    }
                }

                Simulation.TimeStep(fixedDelta);

                while (updates < 1)
                {
                    currentTime = DateTime.UtcNow.Ticks;
                    timePassed = (int)(currentTime - lastUpdate);
                    updates = (((float)timePassed) / (updateRate / Time));
                }
                while (doingframe) { }
                Simulation.SwapBuffers();
                lastUpdate = DateTime.UtcNow.Ticks;
            }
        }

        static bool doingframe = false;
        public static void Update(float delta)
        {
            long currentTime = DateTime.UtcNow.Ticks;
            int timePassed = (int)(currentTime - lastUpdate);
            float updates = (((float)timePassed) / updateRate) * Time;

            while (updates > 1)
            {
                currentTime = DateTime.UtcNow.Ticks;
                timePassed = (int)(currentTime - lastUpdate);
                updates = (((float)timePassed) / updateRate) * Time;
            }

            doingframe = true;
            while (!Simulation.UpdateInterpolation(Maths.Clamp(updates, 0, 1), fixedDelta)) { }
            doingframe = false;

            foreach (var u in updatables)
            {
                u.Update(delta);
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.E))
            {
                Time = 0.1f;
            }
            else if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.R))
            {
                Time = 0.01f;
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
