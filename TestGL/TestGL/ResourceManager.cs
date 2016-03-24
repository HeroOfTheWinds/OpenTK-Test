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
    // A static singleton ResourceManager class that hosts several
    // functions to load Textures and Shaders. Each loaded texture
    // and/or shader is also stored for future reference by string
    // handles. All functions and resources are static and no 
    // public constructor is defined.
    class ResourceManager
    {
        // Resource storage
        public static Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        // Loads (and generates) a shader program from file loading vertex, fragment (and geometry) shader's source code. 
        // If gShaderFile is not null, it also loads a geometry shader
        public static Shader LoadShader(string vShaderFile, string fShaderFile, string gShaderFile, string name)
        {
            Shaders.Add(name, loadShaderFromFile(vShaderFile, fShaderFile, gShaderFile));
            return Shaders[name];
        }

        // Retrieves a stored sader
        public static Shader GetShader(string name)
        {
            return Shaders[name];
        }

        // Loads (and generates) a texture from file
        public static Texture2D LoadTexture(string file, bool alpha, string name)
        {
            Textures.Add(name, loadTextureFromFile(file, alpha));
            return Textures[name];
        }

        // Retrieves a stored texture
        public static Texture2D GetTexture(string name)
        {
            return Textures[name];
        }

        // Properly de-allocates all loaded resources
        public static void Clear()
        {
            // (Properly) delete all shaders	
            foreach (string key in Shaders.Keys)
                GL.DeleteProgram(Shaders[key].ID);
            // (Properly) delete all textures
            foreach (string key in Textures.Keys)
                GL.DeleteTextures(1, new[] { Textures[key].ID });
        }

        
        // Private constructor, that is we do not want any actual resource manager objects. Its members and functions should be publicly available (static).
        private ResourceManager()
        {

        }

        // Loads and generates a shader from file
        private static Shader loadShaderFromFile(string vShaderFile, string fShaderFile, string gShaderFile = null)
        {
            // 1. Retrieve the vertex/fragment source code from filePath
            string vertexCode = null;
            string fragmentCode = null;
            string geometryCode = null;
            try
            {
                // Open files
                vertexCode = File.ReadAllText(vShaderFile);
                fragmentCode = File.ReadAllText(fShaderFile);
                // If geometry shader path is present, also load a geometry shader
                if (gShaderFile != null)
                {
                    geometryCode = File.ReadAllText(gShaderFile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR::SHADER: Failed to read shader files");
            }
            string vShaderCode = vertexCode;
            string fShaderCode = fragmentCode;
            string gShaderCode = geometryCode;
            // 2. Now create shader object from source code
            Shader shader = new Shader();
            shader.Compile(vShaderCode, fShaderCode, gShaderFile != null ? gShaderCode : null);
            return shader;
        }

        // Loads a single texture from file
        private static Texture2D loadTextureFromFile(string file, bool alpha)
        {
            // Create Texture object
            Texture2D texture = new Texture2D();
            if (alpha)
            {
                texture.Internal_Format = PixelInternalFormat.Rgba;
                texture.Image_Format = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
            }
            // Load image
            int width, height;
            // Load the image from file into a bitmap that OpenTK can read easily
            Bitmap bmp = new Bitmap(file);
            BitmapData image = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Get the width and height
            width = bmp.Width;
            height = bmp.Height;

            // Now generate texture
            texture.Generate(width, height, image);
            bmp.UnlockBits(image);

            // Set Texture wrap and filter modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // Unbind texture
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }
    };
}
