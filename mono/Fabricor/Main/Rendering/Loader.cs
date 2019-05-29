using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Fabricor.Main.Rendering
{
    public class Loader
    {
        private List<int> vaos = new List<int>();
        private List<int> vbos = new List<int>();

        private List<Shader> shaders = new List<Shader>();


        public Shader loadShader(string name)
        {
            Shader s = new Shader(name);
            shaders.Add(s);
            return s;
        }

        public RawModel loadToVAO(float[] positions)
        {
            int vaoID = createVAO();
            storeDataInAttributeList(0, positions);
            unbindVAO();
            return new RawModel(vaoID, positions.Length / 3);
        }

        private int createVAO()
        {
            int vaoID = GL.GenVertexArray();
            vaos.Add(vaoID);
            GL.BindVertexArray(vaoID);
            return vaoID;
        }

        private void storeDataInAttributeList(int attribNumber, float[] data)
        {
            int vboID = GL.GenBuffer();
            vbos.Add(vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(attribNumber, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void unbindVAO()
        {
            GL.BindVertexArray(0);
        }

        public void cleanUp()
        {
            foreach (int vao in vaos)
                GL.DeleteVertexArray(vao);

            foreach (int vbo in vbos)
                GL.DeleteBuffer(vbo);

            foreach (Shader s in shaders)
                s.cleanUp();
        }
    }
}
