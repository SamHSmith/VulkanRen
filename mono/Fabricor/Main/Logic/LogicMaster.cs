using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Fabricor.Main.Logic.Grids;
using Fabricor.Main.Logic.Physics;
using Fabricor.Main.Logic.Physics.Shapes;
using Fabricor.Main.Rendering;
using Plane = Fabricor.Main.Logic.Physics.Shapes.Plane;

namespace Fabricor.Main.Logic
{
    public class LogicMaster
    {
        public static List<IUpdatable> updatables = new List<IUpdatable>();
        private static DebugCamera camera= new DebugCamera();

        private static List<Grid> gs = new List<Grid>();


        public static void Init()
        {
            MasterRenderer.Init();
            updatables.Add(camera);

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

            Random r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                Grid g = new Grid();
                g.Put(0, 0, 0, 1);
                g.rb = new Rigidbody();
                g.rb.AddShape(cube);
                g.rb.transform.position = new Vector3((float)r.NextDouble() * 100, (float)r.NextDouble() * 100, (float)r.NextDouble() * 100);
                g.rb.linearVelocity = new Vector3((float)r.NextDouble() * 1, (float)r.NextDouble() * 1, (float)r.NextDouble() * 1);
                g.transform = g.rb.transform;
                updatables.Add(g);
                MasterRenderer.toRenderGrids.Add(g);
                Simulation.rigidbodies.Add(g.rb);
                gs.Add(g);
            }

        }

        public static void Update(float delta)
        {

            foreach (var u in updatables)
            {
                u.Update(delta);
            }
            delta = 1f / 75;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Y)) {
                delta /=5;
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.U))
            {
                delta *= 10;
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.T))
            {

                Simulation.TimeStep(delta);

                foreach (var g in gs)
                {
                    g.rb.linearVelocity += -Vector3.Normalize(g.rb.transform.position)*4 * delta;

                    g.transform = g.rb.transform;
                }
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.P))
            {
                float energy = 0;
                foreach (var rb in Simulation.rigidbodies)
                {
                    energy += rb.Energy;
                }
                Console.WriteLine("ENERGY: "+energy);
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
