using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Fabricor.Main.Rendering
{
    public class Shader
    {
        private string name = "";
        private int shaderProgram = 0, fragmentShader = 0, vertexShader = 0;
        private ShaderAttribute[] attribs;
        private ShaderAttribute[] uniformAttribs;

        public Shader(string name,ShaderAttribute[] attribs, ShaderAttribute[] uniformAttribs)
        {
            this.name = name;
            this.attribs = attribs;
            this.uniformAttribs = uniformAttribs;
            CompileShaders();
        }

        private void CompileShaders()
        {
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name+"_vert")).ReadToEnd());
            GL.CompileShader(vertexShader);

            int status;
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
                throw new Exception(
                    String.Format("Error compiling {0} shader: {1}",
                        status.ToString(), GL.GetShaderInfoLog(vertexShader)));

            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name + "_frag")).ReadToEnd());
            GL.CompileShader(fragmentShader);

            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
            if (status == 0)
                throw new Exception(
                    String.Format("Error compiling {0} shader: {1}",
                        status.ToString(), GL.GetShaderInfoLog(fragmentShader)));

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            BindAttributes();
            GL.LinkProgram(shaderProgram);
            GL.ValidateProgram(shaderProgram);



            BindUniforms();
        }

        public void StartProgram()
        {
            GL.UseProgram(shaderProgram);
        }

        public void StopProgram()
        {
            GL.UseProgram(0);
        }

        private void BindAttributes()
        {
            foreach(ShaderAttribute a in attribs)
            {
                GL.BindAttribLocation(shaderProgram, a.attribute, a.Name);
            }

        }

        private void BindUniforms()
        {
            foreach (ShaderAttribute a in uniformAttribs)
            {
                a.attribute = GL.GetUniformLocation(shaderProgram, a.Name);
                Console.Write(a.attribute);
            }
        }

        public void LoadMatrix(string name, Matrix4 mat)
        {
            GL.UniformMatrix4(getUniformLocation(name), false, ref mat);
        }

        private int getUniformLocation(string uniform)
        {
            foreach(ShaderAttribute a in uniformAttribs)
            {
                if (a.Name.Equals(uniform))
                {
                    return a.attribute;
                }
            }
            return 0;
        }

        public void cleanUp()
        {
            GL.DetachShader(shaderProgram, vertexShader);
            GL.DetachShader(shaderProgram, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteProgram(shaderProgram);
        }

        public int getShaderProgram()
        {
            return shaderProgram;
        }
    }

    public class ShaderAttribute
    {
        public string Name = "";
        public int attribute = 0;

        public ShaderAttribute(string name, int attribute)
        {
            Name = name;
            this.attribute = attribute;
        }
    }

}
