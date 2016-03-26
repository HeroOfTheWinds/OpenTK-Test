using System;
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

        public Game() : base(512, 512, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4))
        {

        }

        void initProgram()
        {
            GL.GenBuffers(1, out ibo_elements);

            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("textured", new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", true));

            activeShader = "textured";

            //textures.Add("LifeIcon.png", loadImage("LifeIcon.png"));

            Sprite tc = new Sprite();
            textures.Add("LifeIcon.png", loadImage("LifeIcon.png", tc));
            tc.TextureID = textures["LifeIcon.png"];
            objects.Add(tc);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            Title = "Hello OpenTK!";

            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            dTime = (float)e.Time;

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            int vertcount = 0;

            foreach (Sprite v in objects)
            {
                //get state of all keyboards on device
                var state = OpenTK.Input.Keyboard.GetState();
                //checks up key, if it is pressed it will check if location is valid then update location.
                if (state[Key.Up])
                {
                    v.Position.Y += 5f / Height;
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
                if (state[Key.Z])
                {
                    v.Rotation.Z += 10f;
                    v.CalculateModelMatrix();
                    v.ModelViewProjectionMatrix = v.ModelMatrix;
                }

                verts.AddRange(v.GetVerts(Width, Height).ToList()); //-------------------------------
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                vertcount += v.VertCount;
                texcoords.AddRange(v.GetTextureCoords());
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

            GL.UseProgram(shaders[activeShader].ProgramID);

            // Clear the buffer binding since we are done with it
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            shaders[activeShader].EnableVertexAttribArrays();

            //GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            int indiceat = 0;

            foreach (Sprite v in objects)
            {
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetAttribute("maintexture"), v.TextureID);
                }

                GL.DrawElements(BeginMode.Quads, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }

            shaders[activeShader].DisableVertexAttribArrays();


            GL.Flush();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreateOrthographic(Width, Height, 1.0f, 64f);

            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadMatrix(ref projection);
        }

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
