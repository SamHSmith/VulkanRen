﻿using System;
using System.Collections.Generic;
using System.IO;
using Fabricor.Main.Logic;
using Fabricor.Main.Logic.Grids;
using Fabricor.Main.Rendering.Loading;
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

        public static Transform camera = new Transform();
        public static List<RenderObject> toRender = new List<RenderObject>();
        public static Loader GlLoader { get { return loader; } }
        private static Loader loader;

        private static Shader shader;

        public static void Init()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            loader = new Loader();

            shader = loader.LoadShader("block",new ShaderAttribute[] {new ShaderAttribute("pos",0),new ShaderAttribute("uvCoords",1) },
                new ShaderAttribute[] { new ShaderAttribute("transform", 0), new ShaderAttribute("persp", 0),
                new ShaderAttribute("view", 0) });

            Chunk c = new Chunk();
            c.blocks[0, 0, 0] = 1;
            c.blocks[1, 0, 0] = 1;
            c.UpdateModel();
            TexturedModel model = new TexturedModel(c.model, new ModelTexture(loader.LoadTexture("BlockTest")));

            toRender.Add(new RenderObject(new Transform(), model));
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

            shader.LoadMatrix("view", camera.ToGLMatrix());
            shader.LoadMatrix("persp", Matrix4.CreatePerspectiveFieldOfView(1.6f, AspectRatio, 0.01f, 1000000));

            foreach (var o in toRender)
            {
                renderModel(o);
            }
            shader.StopProgram();

        }
        private static void renderModel(RenderObject o)
        {
            if (o == null)
                return;

            GL.BindVertexArray(o.Model.RawModel.vaoID);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            shader.LoadMatrix("transform", o.Transform.ToGLMatrix());

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, o.Model.Texture.textureID);
            GL.DrawElements(BeginMode.Triangles, o.Model.RawModel.vertexCount, DrawElementsType.UnsignedInt, 0);
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
