using System;
using System.Collections.Generic;
using System.Drawing;
using Fabricor.Main.Rendering.Models;
using OpenTK.Graphics.OpenGL4;

namespace Fabricor.Main.Rendering
{
    public class Loader
    {
        private List<int> vaos = new List<int>();
        private List<int> vbos = new List<int>();

        private List<Shader> shaders = new List<Shader>();

        private List<string> textureNames = new List<string>();
        private List<int> textures = new List<int>();


        public Shader loadShader(string name,ShaderAttribute[] attributes, ShaderAttribute[] uniforms)
        {
            Shader s = new Shader(name,attributes,uniforms);
            shaders.Add(s);
            return s;
        }

        public RawModel loadToVAO(float[] positions, float[] uvcoords, int[] indices)
        {
            int vaoID = createVAO();
            bindIndicesBuffer(indices);
            storeDataInAttributeList(0,3, positions);
            storeDataInAttributeList(1, 2, uvcoords);
            unbindVAO();
            return new RawModel(vaoID, indices.Length);
        }

        private int createVAO()
        {
            int vaoID = GL.GenVertexArray();
            vaos.Add(vaoID);
            GL.BindVertexArray(vaoID);
            return vaoID;
        }

        private void storeDataInAttributeList(int attribNumber, int stride, float[] data)
        {
            int vboID = GL.GenBuffer();
            vbos.Add(vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribNumber, stride, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void unbindVAO()
        {
            GL.BindVertexArray(0);
        }

        private void bindIndicesBuffer(int[] indices)
        {
            int vboID = GL.GenBuffer();
            vbos.Add(vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);
            
        }

        public int LoadTexture(string filename)
        {
            if (textureNames.Contains(filename))
            {
                return textures[textureNames.IndexOf(filename)];
            }

            int width, height;
            var data = LoadTextureFromFile(filename, out width, out height);
            int texture;
            GL.CreateTextures(TextureTarget.Texture2D, 1, out texture);
            GL.TextureStorage2D(
                texture,
                1,                           // levels of mipmapping
                SizedInternalFormat.Rgba32f, // format of texture
                width,
                height);

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TextureSubImage2D(texture,
                0,                  // this is level 0
                0,                  // x offset
                0,                  // y offset
                width,
                height,
                PixelFormat.Rgba,
                PixelType.Float,
                data);

            textureNames.Add(filename);
            textures.Add(texture);

            return texture;
            // data not needed from here on, OpenGL has the data
        }

        private float[] LoadTextureFromFile(string filename, out int width, out int height)
        {
            float[] r;
            using (var bmp = (Bitmap)Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Fabricor.Resources." + filename + ".png"),true))
            {
                width = bmp.Width;
                height = bmp.Height;
                r = new float[width * height * 4];
                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var pixel = bmp.GetPixel(x, y);
                        r[index++] = pixel.R / 255f;
                        r[index++] = pixel.G / 255f;
                        r[index++] = pixel.B / 255f;
                        r[index++] = pixel.A / 255f;
                    }
                }
            }
            return r;
        }


        public void cleanUp()
        {
            foreach (int vao in vaos)
                GL.DeleteVertexArray(vao);

            foreach (int vbo in vbos)
                GL.DeleteBuffer(vbo);

            foreach (Shader s in shaders)
                s.cleanUp();

            foreach (int t in textures)
                GL.DeleteTexture(t);
        }
    }
}
