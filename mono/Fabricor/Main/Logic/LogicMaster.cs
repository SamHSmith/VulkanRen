using System;
using System.Collections.Generic;
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
            rb1.transform.position.Y = 2;
            rb1.transform.position.Z = 0.0f;
            rb1.transform.position.X = 0.1f;
            rb1.linearVelocity.Y = -0.2f;
            rb1.angularVelocity.X = 1f;
            rb1.angularVelocity.Y = 1f;
            Simulation.rigidbodies.Add(rb);
            Simulation.rigidbodies.Add(rb1);
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
            rb.AddShape(cube);
            rb1.AddShape(cube);
            

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
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Y)) {
                delta /=11;
            }

                if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.T))
            {
                Simulation.TimeStep(delta);
                rb.linearVelocity += Vector3.Normalize(rb1.transform.position - rb.transform.position) * delta / 2;
                rb1.linearVelocity += Vector3.Normalize(rb.transform.position - rb1.transform.position) * delta / 2 ;
            }
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
