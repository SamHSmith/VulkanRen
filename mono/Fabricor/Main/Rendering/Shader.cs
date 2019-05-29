using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace Fabricor.Main.Rendering
{
    public class Shader
    {
        private string name = "";
        private int shaderProgram = 0;

        public Shader(string name)
        {
            this.name = name;
            shaderProgram = CompileShaders();
        }

        private int CompileShaders()
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name+"_vert")).ReadToEnd());
            GL.CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name + "_frag")).ReadToEnd());
            GL.CompileShader(fragmentShader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        public void cleanUp()
        {
            GL.DeleteProgram(shaderProgram);
        }

        public int getShaderProgram()
        {
            return shaderProgram;
        }
    }

}
