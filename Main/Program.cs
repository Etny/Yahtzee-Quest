using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System;
using System.Drawing;
using Yahtzee.Game;
using Yahtzee.Render;

namespace Yahtzee.Main
{
    class Program
    {

        public static Window Window;
        public static Scene Scene;
        public static InputManager InputManager;

        private static GL gl;

        static void Main(string[] args)
        {
            Window = new Window();
            if (!Window.OpenWindow("Yahtzee Quest", new Size(1280, 720)))
                return;
            gl = GL.GetApi();
            Window.SetVSync(true);
            Window.OnTick += Tick;
            Window.OnButton += OnButton;
            Window.OnResize += OnResize;

            SetupGL();

            InputManager = new InputManager();

            Scene = new Scene();

            Window.StartLoop();
        }

        private static void OnResize(int width, int height)
        {
            gl.Viewport(new Size(width, height));
        }

        private static void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Escape)
                Window.Close();
        }

        private static void Tick(double deltaTime)
        {
            Scene.Update(deltaTime);
            Scene.Render();

            Window.EndRender();
        }

        private static void SetupGL()
        {
            gl.Enable(EnableCap.CullFace);
            gl.Enable(EnableCap.DepthTest);
        }

    }
}
