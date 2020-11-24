using GlmSharp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.UI.RenderComponent
{
    class ImageBlurRenderComponent : RenderComponent
    {
        public ImageTexture Image { get; }
        public vec4 Tint { get; protected set; } = new vec4(1);

        public ImageBlurRenderComponent(GL Gl, string imgPath)
        {
            _shader = ShaderRepository.GetShader("UI/UI", "UI/UIBlurImage");

            Image = new ImageTexture(Gl, imgPath, TextureType.Other);
        }

        public override void Draw(QuadComponent comp)
        {
            Image.BindToUnit(1);
            _shader.SetVec4("tint", Tint);
            _shader.SetInt("image", 1);
            _shader.SetInt("screen", 0);

            base.Draw(comp);
        }

        public void SetTint(vec4 tint)
            => Tint = tint;
        
    }
}
