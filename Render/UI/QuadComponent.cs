using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using Yahtzee.Core;
using Yahtzee.Game;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.UI
{
    abstract class QuadComponent : IUIComponent
    {
        public Transform2D Transform = Transform2D.Identity;
        public vec2 Position { get { return Transform.Translation; } set { Transform.Translation = value; } }

        public QuadMesh Quad { get; protected set; }

        protected Shader _shader;

        protected UILayer _layer;


        public QuadComponent(UILayer layer) { _layer = layer; _shader = ShaderRepository.GetShader("UI/UI", "UI/UIDefault"); }

        public QuadComponent(UILayer layer, vec2 size)
        {
            _layer = layer;

            Quad = new QuadMesh(size.ScaleToScreen());
            _shader = ShaderRepository.GetShader("UI/UI", "UI/UIDefault");
        }


        public virtual void Draw()
        {
            _shader.SetMat4("model", Transform.ModelMatrixUI);
            _shader.SetVec2("screenSize", _layer.UIFrameBuffer.BoundTexture.Size);
            Quad.Draw(_shader);
        }
        

        public abstract void Update(Time deltaTime);
    }
}
