using System;
using Yahtzee.Render;

namespace Yahtzee.Main
{
    class Program
    {

        public static Window Window;
        public static Scene Scene;

        private static double total;

        static void Main(string[] args)
        {
            Window = new Window();
            if (!Window.OpenWindow("Yahtzee Quest", new System.Drawing.Size(1280, 720)))
                return;

            Scene = new Scene();

            Window.OnTick += Tick;
            Window.StartLoop();
        }

        private static void Tick(double deltaTime)
        {
            Scene.Update(deltaTime);
            Scene.Render();

            total += deltaTime;
            if (total >= 5) Window.Close();

            Window.EndRender();
        }

    }
}
