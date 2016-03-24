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
    class Texture2D
    {
        // Holds the ID of the texture object, used for all texture operations to reference to this particlar texture
        public int ID;
        // Texture image dimensions
        public int width, height; // Width and height of loaded image in pixels

        // Texture Format
        public PixelInternalFormat Internal_Format; // Format of texture object
        public OpenTK.Graphics.OpenGL.PixelFormat Image_Format; // Format of loaded image

        // Texture configuration
        public int Wrap_S; // Wrapping mode on S axis
        public int Wrap_T; // Wrapping mode on T axis
        public int Filter_Min; // Filtering mode if texture pixels < screen pixels
        public int Filter_Max; // Filtering mode if texture pixels > screen pixels

        // Constructor (sets default texture modes)
        public Texture2D()
        {
            width = 0;
            height = 0;
            Internal_Format = PixelInternalFormat.Rgb;
            Image_Format = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
            Wrap_S = (int) TextureWrapMode.Repeat;
            Wrap_T = (int) TextureWrapMode.Repeat;
            Filter_Min = (int)TextureMinFilter.Linear;
            Filter_Max = (int)TextureMinFilter.Linear;
            GL.GenTextures(1, out ID);
        }

        // Generates texture from image data
        public void Generate(int w, int h, BitmapData bmp_data)
        {
            width = w;
            height = h;

            // Create Texture
            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            
        }

        // Binds the texture as the current active GL.Texture2D texture object
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, ID);
        }
    }
}
