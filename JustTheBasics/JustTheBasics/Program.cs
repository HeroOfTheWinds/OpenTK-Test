﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using OpenTK.Input;

namespace JustTheBasics
{
    class Game: GameWindow
    {
        // Var to hold time since last frame
        float dTime = 0.0f;

        // Dictionary of named shader programs from our custom class
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";

        // Camera we're viewing out of
        Camera cam = new Camera();

        // Index buffer object elements
        int ibo_elements;

        // More info for the VBO
        Vector3[] vertdata;
        Vector3[] coldata;
        int[] indicedata;
        Vector2[] texcoorddata;

        // List of all the sprites we will be rendering
        List<Sprite> objects = new List<Sprite>();

        // Dictionary to store texture ID's by name
        Dictionary<string, int> textures = new Dictionary<string, int>();

        // Default constructor that sets window size to 512x512 and adds some antialiasing (the GraphicsMode part)
        public Game() : base(512, 512, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4))
        {

        }

        // Use this function to load images, objects, and shaders at the start
        void initProgram()
        {
            // Generate a buffer on the graphics card for VBO indices
            GL.GenBuffers(1, out ibo_elements);

            // Load two shaders, one for test squares and the other for textured sprites
            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("textured", new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", true));

            // Declare that the shader we will use first is the textured sprite
            activeShader = "textured";

            // Create a new sprite here
            // In final compiled game, should actually make gameObjects here and create the sprites
            // inside those.  For now, Sprite and GameObject will be synonymous.
            Sprite tc = new Sprite();

            // Load one of our images (also try Black_Hole.png), bind it to tc
            textures.Add("LifeIcon.png", loadImage("LifeIcon.png", tc));
            tc.TextureID = textures["LifeIcon.png"];

            // Add the sprite to our list of active Sprites
            objects.Add(tc);
        }

        // This function overrides the base OnLoad function
        // Set window parameters here like background and title
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Call our initialization function
            initProgram();

            Title = "Hello OpenTK!";

            GL.ClearColor(Color.CornflowerBlue); // Yech, but at least it makes it easy to see mistakes.
            GL.PointSize(5f);
        }

        // Override the OnUpdateFrame function of the GameWindow class
        // Here we put logical updates for each frame.
        // Start with game logic for the global room, then put code for each object
        // in the loop over Sprites in objects.
        // Don't touch the remainder, that sets up the visuals.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Calculate the time since last call of OnUpdateFrame
            dTime = (float)e.Time;

            // Important lists for the Vertex Buffer Objects (VBOs)
            List<Vector3> verts = new List<Vector3>(); // Vertices of the quads
            List<int> inds = new List<int>(); // Indices, closely tied to above
            List<Vector3> colors = new List<Vector3>(); // Not used by textured sprites
            List<Vector2> texcoords = new List<Vector2>(); // Coordinates of the texture in quad space

            // Total number of processed vertices
            int vertcount = 0;

            // Loop over every sprite in the game (later should be GameObject)
            foreach (Sprite v in objects)
            {
                //get state of all keyboards on device
                var state = OpenTK.Input.Keyboard.GetState();
                //checks up key, if it is pressed it will update location.
                if (state[Key.Up])
                {
                    // Set the sprite's position
                    v.Position.Y += 5f / Height;
                    // Update the matrix used to calculate the Sprite's visuals
                    v.CalculateModelMatrix();
                    v.ModelViewProjectionMatrix = v.ModelMatrix;
                }
                if (state[Key.Down])
                {
                    v.Position.Y -= 5f / Height;
                    v.CalculateModelMatrix();
                    v.ModelViewProjectionMatrix = v.ModelMatrix;
                }
                if (state[Key.Right])
                {
                    v.Position.X += 5f / Height;
                    v.CalculateModelMatrix();
                    v.ModelViewProjectionMatrix = v.ModelMatrix;
                }
                if (state[Key.Left])
                {
                    v.Position.X -= 5f / Height;
                    v.CalculateModelMatrix();
                    v.ModelViewProjectionMatrix = v.ModelMatrix;
                }

                // Populate the previously defined lists
                verts.AddRange(v.GetVerts(Width, Height).ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                vertcount += v.VertCount;
                texcoords.AddRange(v.GetTextureCoords());
            }

            // Convert the lists into easier to use arrays
            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            // Use a VBO to set up the vertex positions of the quads
            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // If there are color parameters, apply them to the shader.
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // If there are texture parameters, also do VBO operations on them
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            // One more VBO operation, this one for aforementioned indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

            // Tell the program to use the Shader we currently are using
            GL.UseProgram(shaders[activeShader].ProgramID);

            // Clear the buffer binding since we are done with it
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Another override, handles the rendering of each frame, which is separate from update frames.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Set the viewport, i.e. the size of the contents of the window to be rendered.
            GL.Viewport(0, 0, Width, Height);
            // Clear the graphics drawn last frame to avoid weird effects.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Enable several important switches to be able to draw flat images and make a generally pretty picture.
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            // Since blending is enabled, give it an alpha (transparency) based function to work with
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Allow vertex attribute arrays to be created on th GPU for this shader
            shaders[activeShader].EnableVertexAttribArrays();

            // Index counter, since we turned some lists into arrays and need to offset accordingly
            int indiceat = 0;

            // Again, this should be GameObjects later.
            foreach (Sprite v in objects)
            {
                // Tell OpenTK to associate the given texture to the VBO we're drawing
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                // Send our projection matrix to the GLSL shader
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                // If shader uses textures, send the image to the shader code for processing
                if (shaders[activeShader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetAttribute("maintexture"), v.TextureID);
                }

                // Draw a square/rectangle
                GL.DrawElements(BeginMode.Quads, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                // Increment our index counter by the number of indices processed
                indiceat += v.IndiceCount;
            }

            // Free up the memory off the GPU
            shaders[activeShader].DisableVertexAttribArrays();

            // Draw the final buffer (or canvas) to screen
            GL.Flush();
            SwapBuffers();
        }

        // Override for resizing the window, not much should be changed here.
        protected override void OnResize(EventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreateOrthographic(Width, Height, 1.0f, 64f);

            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadMatrix(ref projection);
        }

        // Function to generate a texture ID for later reference, associated with each bitmap we load
        // Don't change anything, there is no reason to.
        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        // Overload for above function that handles reading a bitmap from file.
        // Not limited to .bmp, can be most formats, check the C# documentation on Bitmaps for full list.
        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException e)
            {
                return -1;
            }
        }

        // Another overload, does the same as above, but also takes the Sprite object we're loading
        // into as an argument, so that it can set the height and width of the sprite.
        int loadImage(string filename, Sprite spr)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                spr.Width = file.Width;
                spr.Height = file.Height;
                return loadImage(file);
            }
            catch (FileNotFoundException e)
            {
                return -1;
            }
        }
    }

    // Small class that basically is a Main() function and just runs the game.
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {

                game.Run();

            }
        }
    }
}
