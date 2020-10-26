using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Game;
using Yahtzee.Main;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.UI
{
    class QuadComponent : IUIComponent
    {
        public Transform2D Transform = Transform2D.Identity;
        public vec2 Position { get { return Transform.Translation; } set { Transform.Translation = value; } }

        public QuadMesh Quad { get; protected set; }

        protected Shader DefaultShader;
        private ImageTexture texture;

        private UILayer _layer;

        public QuadComponent(UILayer layer)
        {
            DefaultShader = ShaderRepository.GetShader("UI/UI", "UI/UIDefault");

            Transform.Translation += new vec2(0, -.5f);
            texture = new ImageTexture(layer.Gl, "Resource/Images/UI/Buttons/Reroll.png", TextureType.Other);

            vec2 size = new vec2(1, texture.GetAspectRatio().y) * .35f;
            Quad = new QuadMesh(size);

            _layer = layer;
        }


        public void Draw()
        {
            DefaultShader.Use();
            DefaultShader.SetMat4("model", Transform.ModelMatrix);
            DefaultShader.SetVec2("screenSize", _layer.UIFrameBuffer.BoundTexture.Size);
            DefaultShader.SetInt("diff", 1);
            texture.BindToUnit(1);
            DefaultShader.SetInt("screen", 0);
            Quad.Draw(DefaultShader);
        }

        public void Update(Time deltaTime)
        {
            //Transform.Orientation += deltaTime.DeltaF * 90f.AsRad();
        }
    }
}
