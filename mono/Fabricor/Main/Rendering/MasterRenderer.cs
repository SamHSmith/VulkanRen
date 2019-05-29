using System;
using System.IO;
using Fabricor.Main.Rendering.Models;
using Fabricor.Main.Rendering.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Fabricor.Main.Rendering
{
    public class MasterRenderer
    {
        public static float AspectRatio = 1;

        private static Loader loader;

        private static Shader shader;

        private static TexturedModel model;

        public static void Init()
        {
            loader = new Loader();

            shader = loader.loadShader("block",new ShaderAttribute[] {new ShaderAttribute("pos",0),new ShaderAttribute("uvCoords",1) },
                new ShaderAttribute[] { new ShaderAttribute("transform", 0), new ShaderAttribute("persp", 0),
                new ShaderAttribute("view", 0) });

            float[] vertices = { -0.5f, 0.5f, 0f, -0.5f, -0.5f, 0f, 0.5f, -0.5f, 0f, 0.5f, 0.5f, 0f};

            int[] indices = { 0, 1, 3, 3, 1, 2 };

            float[] texcoords = { 0, 0, 0, 1, 1, 1, 1, 0 };

            RawModel rawmodel = loader.loadToVAO(vertices, texcoords, indices);
            model = new TexturedModel(rawmodel, new ModelTexture(loader.LoadTexture("Dirt")));

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

            shader.StartProgram();


            renderModel(model);
            shader.StopProgram();

        }
        static float distance = 0, rot = 0;
        private static void renderModel(TexturedModel m)
        {
            distance -= 0.01f;
            rot += 0.05f;

            GL.BindVertexArray(m.RawModel.vaoID);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            shader.LoadMatrix("transform", Matrix4.CreateRotationY(rot));
            shader.LoadMatrix("view", Matrix4.CreateTranslation(new Vector3(0, 0, distance)));
            shader.LoadMatrix("persp", Matrix4.CreatePerspectiveFieldOfView(1.6f,AspectRatio,0.01f,1000000));

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, m.Texture.textureID);
            GL.DrawElements(BeginMode.Triangles, m.RawModel.vertexCount, DrawElementsType.UnsignedInt, 0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
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
