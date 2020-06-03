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

        private Shader lightingShader;
        private Shader drawShader;

        public Scene()
        {

        }

        public void Render()
        {

        }

        public void Update(double deltaTime)
           => Entities.ForEach(e => e.Update(deltaTime));

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));
        
    }
}
