using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
//using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace TestGL
{
    class Game
    {
        // String denoting what the current status of the game is, i.e. paused, at menu, loading, running...
        public string state;

        // Ints for height and width of the screen. (Should be set in room designer.)
        public int height, width;

        public SpriteRenderer Renderer = new SpriteRenderer();

        // Constructors and destructor
        public Game()
        {

        }
        public Game(int ht, int wd)
        {
            height = ht;
            width = wd;
        }
        ~Game()
        {
            
        }

        // Initialize the game, loading textures and resources as well
        public void Initialize()
        {
            // Load shaders
            ResourceManager.LoadShader("C:/Users/Cor/Documents/Visual Studio 2015/Projects/TestGL/TestGL/shaders/sprite.vs", "C:/Users/Cor/Documents/Visual Studio 2015/Projects/TestGL/TestGL/shaders/sprite.frag", null, "sprite");
            // Configure shaders
            Matrix4 projection = Matrix4.CreateOrthographic((float) width, (float) height, 0.0f, -1.0f);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            ResourceManager.GetShader("sprite").Use().SetInteger("image", 0);
            ResourceManager.GetShader("sprite").SetMatrix4("projection", projection);
            // Load textures
            ResourceManager.LoadTexture("C:/Users/Cor/Documents/Visual Studio 2015/Projects/TestGL/TestGL/textures/boy.png", true, "boy");
            // Set render-specific controls
            Shader myShader = ResourceManager.GetShader("sprite");
            Renderer = new SpriteRenderer(myShader);
        }

        // Called every frame, takes time since last Update as argument
        public void Update(double deltaTime)
        {

        }

        // Code for rendering the current room goes here
        public void Render()
        {
            Texture2D myTexture = ResourceManager.GetTexture("boy");

            Renderer.DrawSprite(myTexture, new Vector2(200, 200), new Vector2(300, 400), 45.0f, new Vector3(0.0f, 1.0f, 0.0f));

        }

        public int LoadTexture(string file)
        {
            Bitmap bitmap = new Bitmap(file);

            int tex;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }
    }
}
