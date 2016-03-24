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
    class MainWindow
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            // The Width of the screen
            const int SCREEN_WIDTH = 800;
            // The height of the screen
            const int SCREEN_HEIGHT = 600;

            //Game MyGame = new Game(SCREEN_WIDTH, SCREEN_HEIGHT);
            Game MyGame = null;

            // Set the background color, a.k.a. the Clear Color
            Color ClearColor = Color.CornflowerBlue; // Will be user's variable later

            // DeltaTime variables (Variables holding time lapsed since last frame/action)
            float deltaTime = 0.0f;
            float lastFrame = 0.0f;

            // Start Game within Menu State
            //MyGame.state = "GAME_ACTIVE";

            deltaTime = 1.0f / 60.0f; // temporary

            //MyGame.ProcessInput(deltaTime);

            using (var game = new GameWindow(SCREEN_WIDTH, SCREEN_HEIGHT, OpenTK.Graphics.GraphicsMode.Default, "Debug Game", GameWindowFlags.Default, DisplayDevice.Default, 3, 3, OpenTK.Graphics.GraphicsContextFlags.Default))
            {
                game.Load += (sender, e) =>
                {
                    // OpenGL configuration
                    /*GL.MatrixMode(MatrixMode.Projection);
                    */
                    GL.LoadIdentity();
                    
                    GL.Ortho(0, SCREEN_WIDTH, 0, SCREEN_HEIGHT, -1, 1);
                    GL.Viewport(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

                    /*GL.MatrixMode(MatrixMode.Modelview);

                    GL.LoadIdentity();

                    GL.ClearColor(Color.White);*/

                    GL.Enable(EnableCap.CullFace);
                    GL.Enable(EnableCap.Blend);
                    GL.Enable(EnableCap.Texture2D);
                    GL.Disable(EnableCap.Lighting);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                    MyGame = new Game(SCREEN_WIDTH, SCREEN_HEIGHT);
                    MyGame.state = "GAME_ACTIVE";

                    game.VSync = VSyncMode.On;

                    /*
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.Off;

                    // Set BG Color
                    GL.ClearColor(ClearColor);

                    // Enable use of 2D Textures
                    GL.Enable(EnableCap.Texture2D);

                    GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
                    
                    int[] viewPort = new int[4];
                    GL.GetInteger(GetPName.Viewport, viewPort);
                    GL.Enable(EnableCap.Blend);
                    
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.Viewport(viewPort[0], viewPort[1], viewPort[2], viewPort[3]);
                    GL.Ortho(viewPort[0], viewPort[0] + viewPort[2], viewPort[1] + viewPort[3], viewPort[1], -1, 1);
                    */

                    MyGame.Initialize();
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // Get time since last update
                    double dTime = e.Time;

                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }

                    MyGame.Update(dTime);
                };

                game.RenderFrame += (sender, e) =>
                {
                    // Get time since last render call
                    double dTime = e.Time;

                    Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

                    GL.MatrixMode(MatrixMode.Modelview);

                    GL.LoadMatrix(ref modelview);

                    // Redraw background
                    GL.ClearColor(ClearColor);

                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    MyGame.Render();
                    //int texture = MyGame.LoadTexture("C:/Users/Cor/Documents/Visual Studio 2015/Projects/TestGL/TestGL/textures/boy.png");
                    //DrawImage(texture);

                    /*GL.Begin(PrimitiveType.Triangles);

                    GL.Color3(1.0f, 0.0f, 0.0f);
                    GL.Vertex3(-1.0f, -1.0f, 0.0f);

                    GL.Color3(0.0f, 1.0f, 0.0f);
                    GL.Vertex3(1.0f, -1.0f, 0.0f);

                    GL.Color3(0.0f, 0.0f, 1.0f);
                    GL.Vertex3(0.0f, 1.0f, 0.0f);

                    GL.End();*/

                    //GL.Flush();
                    game.SwapBuffers();
                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }

            return;
        }

        

        public static void DrawImage(int image)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Ortho(0, 800, 0, 600, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.Lighting);

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, image);

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex3(256, 0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex3(256, 256, 0);

            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 256, 0);

            GL.End();

            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}