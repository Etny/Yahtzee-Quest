using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using Yahtzee.Main;
using GlmSharp;

namespace Yahtzee.Game
{
    class ModelEntity : Entity
    {

        public Model Model;

        public ModelEntity(string modelPath) : base() { Model = new Model(modelPath); }

        public override void Draw(Shader shader)
        {
            shader.SetMat4("model", Transform.ModelMatrix);
            Model.Draw(shader);
        }
    }
}
