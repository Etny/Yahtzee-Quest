using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;

namespace Yahtzee.Render
{
    class Scene
    {
        List<Entity> Entities = new List<Entity>();
        List<Light> Lights = new List<Light>();

        public Camera Camera;

        private Shader lightingShader;
        private Shader defaultShader;

        private GL gl;

        public Scene()
        {
            gl = GL.GetApi();

            defaultShader = new Shader("default");

            Camera = new Camera();

            gl.Enable(EnableCap.DepthTest);

            Entities.Add(new Entity("Backpack/backpack.obj"));
        }

        public void Render()
        {
            gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));

            defaultShader.Use();
            Camera.SetMatrices();
            RenderScene(defaultShader);
        }

        public void Update(double deltaTime)
        {
            Camera.Update(deltaTime);
            Entities.ForEach(e => e.Update(deltaTime));
        }

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));
        
    }
}
