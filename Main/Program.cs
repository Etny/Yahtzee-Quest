using Silk.NET.GLFW;
using System;
using Yahtzee.Game;
using Yahtzee.Render;

namespace Yahtzee.Main
{
    class Program
    {

        public static Window Window;
        public static Scene Scene;
        public static InputManager InputManager;

        static void Main(string[] args)
        {
            Window = new Window();
            if (!Window.OpenWindow("Yahtzee Quest", new System.Drawing.Size(1280, 720)))
                return;

            InputManager = new InputManager();

            Scene = new Scene();

            Window.OnTick += Tick;
            Window.OnButton += OnButton;
            Window.StartLoop();
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



    }
}
