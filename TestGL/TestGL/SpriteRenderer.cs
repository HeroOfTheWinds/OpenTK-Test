using System;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
//using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Collections.Generic;

namespace TestGL
{
    class SpriteRenderer
    {
        public SpriteRenderer()
        {
            // Empty constructor
        }

        // Constructor (inits shaders/shapes)
        public SpriteRenderer(Shader shader)
        {

        }
        
        // Destructor
        ~SpriteRenderer()
        {

        }
        
        // Renders a defined quad textured with given sprite
        public void DrawSprite(Texture2D texture, Vector2 position, Vector2 size = new Vector2(), float rotate = 0.0f, Vector3 color = new Vector3())
        {
            // Default values if none passed
            if (size == null)
            {
                size = new Vector2(10f, 10f);
            }
            if (color == null)
            {
                color = new Vector3(1.0f, 1.0f, 1.0f);
            }

            // Prepare transformations
            shader.Use();
            Matrix4 model = Matrix4.Identity;
            model = Matrix4.CreateTranslation(new Vector3(position[0], position[1], 0.0f));  // First translate (transformations are: scale happens first, then rotation and then finall translation happens; reversed order)

            model *= Matrix4.CreateTranslation(new Vector3(0.5f * size[0], 0.5f * size[1], 0.0f)); // Move origin of rotation to center of quad
            model *= Matrix4.CreateRotationZ(rotate); // Then rotate
            model *= Matrix4.CreateTranslation(new Vector3(-0.5f * size[0], -0.5f * size[1], 0.0f)); // Move origin back

            model = Matrix4.CreateScale(size[0], size[1], 1.0f); // Last scale
            
            shader.SetMatrix4("model", model);

            // Render textured quad
            shader.SetVector3f("spriteColor", color);
            GL.ActiveTexture(TextureUnit.Texture0);
            texture.Bind();

            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
            GL.Finish();
            
        }
        
        // Render state
        private Shader shader = new Shader();

        private uint quadVAO;

        // Initializes and configures the quad's buffer and vertex attributes
        private void initRenderData()
        {
            // Configure VAO/VBO
            int VBO;
            float[] vertices = { 
                // Pos      // Tex
                0.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 0.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 0.0f
            };

            GL.GenVertexArrays(1, out quadVAO);
            GL.GenBuffers(1, out VBO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(quadVAO);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), (IntPtr) null);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}
