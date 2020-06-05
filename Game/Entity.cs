using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using GlmSharp;

namespace Yahtzee.Game
{
    class Entity
    {

        public Model Model;

        public Entity(string modelPath)
        {
            Model = new Model(modelPath);
        }

        public void Update(double deltaTime)
        {

        }

        public void Draw(Shader shader)
        {
            shader.SetMat4("model", mat4.Identity);
            Model.Draw(shader);
        }
    }
}
