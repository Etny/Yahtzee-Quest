using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Game;
using Yahtzee.Main;

namespace Yahtzee.Render.UI
{
    class QuadComponent : IUIComponent
    {
        public Transform2D Transform = Transform2D.Identity;
        public vec2 Position { get { return Transform.Translation; } set { Transform.Translation = value; } }

        public QuadMesh Quad { get; protected set; }

        protected Shader DefaultShader;

        public QuadComponent()
        {
            DefaultShader = ShaderRepository.GetShader("UI/UI", "UI/UIDefault");

            Quad = new QuadMesh(.4f, .1f);
        }


        public void Draw()
        {
            DefaultShader.Use();
            DefaultShader.SetMat4("model", Transform.ModelMatrix);
            Quad.Draw(DefaultShader);
        }

        public void Update(Time deltaTime)
        {
            //Transform.Orientation += deltaTime.DeltaF * 90f.AsRad();
        }
    }
}
