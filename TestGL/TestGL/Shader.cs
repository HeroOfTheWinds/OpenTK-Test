using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
//using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace TestGL
{
    class Shader
    {
        // State
        public int ID;
        // Constructor
        public Shader()
        {
            // Nothin' doin.
        }

        // Sets the current shader as active
        public Shader Use()
        {
            GL.UseProgram(this.ID);
            return this;
        }

        // Compiles the shader from given source code
        public void Compile(string vertexSource, string fragmentSource, string geometrySource = null)
        {
            //int sVertex, sFragment;
            int gShader = 0;

            // Define and compile a Vertex Shader
            int sVertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(sVertex, vertexSource);
            GL.CompileShader(sVertex);
            checkCompileErrors(sVertex, "VERTEX");

            // Define and compile a Fragment Shader
            int sFragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(sFragment, fragmentSource);
            GL.CompileShader(sFragment);
            checkCompileErrors(sFragment, "FRAGMENT");

            // If geometry shader source code is given, also compile geometry shader
            if (geometrySource != null)
            {
                gShader = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(gShader, geometrySource);
                GL.CompileShader(gShader);
                checkCompileErrors(gShader, "GEOMETRY");
            }

            // Create a Shader Program
            ID = GL.CreateProgram();

            // Attach each of our shaders to it to create the final render directives
            GL.AttachShader(ID, sVertex);
            GL.AttachShader(ID, sFragment);
            if (geometrySource != null)
                GL.AttachShader(ID, gShader);
            // Bind the shader program to this object so that the render can be done
            GL.LinkProgram(ID);
            checkCompileErrors(ID, "PROGRAM"); // Make sure no errors were thrown

            // Delete the shaders, as they're linked into our program now and no longer necessary
            GL.DeleteShader(sVertex);
            GL.DeleteShader(sFragment);
            if (geometrySource != null)
                GL.DeleteShader(gShader);
        }

        // Utility functions
        public void SetFloat(string name, float value, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform1(GL.GetUniformLocation(ID, name), value);
        }

        public void SetInteger(string name, int value, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform1(GL.GetUniformLocation(ID, name), value);
        }

        public void SetVector2f(string name, float x, float y, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform2(GL.GetUniformLocation(ID, name), x, y);
        }

        public void SetVector2f(string name, Vector2 value, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform2(GL.GetUniformLocation(ID, name), value[0], value[1]);
        }

        public void SetVector3f(string name, float x, float y, float z, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform3(GL.GetUniformLocation(ID, name), x, y, z);
        }

        public void SetVector3f(string name, Vector3 value, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform3(GL.GetUniformLocation(ID, name), value[0], value[1], value[2]);
        }

        public void SetVector4f(string name, float x, float y, float z, float w, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform4(GL.GetUniformLocation(ID, name), x, y, z, w);
        }

        public void SetVector4f(string name, Vector4 value, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.Uniform4(GL.GetUniformLocation(ID, name), value[0], value[1], value[2], value[3]);
        }

        public void SetMatrix4(string name, Matrix4 matrix, bool useShader = false)
        {
            if (useShader)
                Use();
            GL.UniformMatrix4(GL.GetUniformLocation(ID, name), false, ref matrix);
        }
        
        // Checks if compilation or linking failed and if so, print the error logs
        void checkCompileErrors(int obj, string type)
        {
            int success;
            string infoLog;

            if (type != "PROGRAM")
            {
                GL.GetShader(obj, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    GL.GetShaderInfoLog(obj, out infoLog);
                    Console.WriteLine("| ERROR::SHADER: Compile-time error: Type: " + type.ToString());
                    Console.WriteLine(infoLog.ToString());
                    Console.WriteLine(" -- --------------------------------------------------- -- ");
                }
            }
            else
            {
                success = (int) GetProgramParameterName.LinkStatus;
                if (success == 0)
                {
                    GL.GetProgramInfoLog(obj, out infoLog);
                    Console.WriteLine("| ERROR::Shader: Link-time error: Type: " + type.ToString());
                    Console.WriteLine(infoLog.ToString());
                    Console.WriteLine(" -- --------------------------------------------------- -- ");
                }
            }
        }
    }
}
