using System;
using System.IO;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Fabricor.Main.Rendering
{
    public class MasterRenderer
    {
        private static Loader loader;

        private static Shader shader;

        private static RawModel model;

        public static void Init()
        {
            loader = new Loader();

            shader = loader.loadShader("block");

            float[] vertices = { -0.5f, 0.5f, 0f, -0.5f, -0.5f, 0f, 0.5f, -0.5f, 0f, 0.5f, -0.5f, 0f, 0.5f, 0.5f, 0f, -0.5f, 0.5f, 0f };

            model = loader.loadToVAO(vertices);
        }

        public static void CleanUp()
        {
            loader.cleanUp();
        }


        public static void MasterRender(float delta)
        {
            totalDelta += delta;
            frameCount++;
            prepare();

            renderRawModel(model);


        }

        private static void renderRawModel(RawModel m)
        {
            GL.UseProgram(shader.getShaderProgram());

            GL.BindVertexArray(m.vaoID);
            GL.EnableVertexAttribArray(0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, m.vertexCount);
            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);
        }

        public static float fps = 0;
        private static float totalDelta = 0;
        private static int frameCount = 0;
        private static void prepare()
        {

            if (totalDelta > 2)
            {
                fps = 1 / (totalDelta / frameCount);
                Console.WriteLine($"FPS: {fps}");
                totalDelta = 0;
                frameCount = 0;
            }

            //Rendering starts here

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.1f;
            backColor.G = 0.1f;
            backColor.B = 0.3f;
            GL.ClearColor(backColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
