using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Models;

namespace Yahtzee.Render.UI.RenderComponent
{
    class BasicRenderComponent : RenderComponent
    {
        public vec3 Color { get; protected set; } = new vec3(0.5f);

        public BasicRenderComponent()
        {
            _shader = ShaderRepository.GetShader("UI/UI", "UI/UIDefault");
        }

        public override void Draw(QuadComponent comp)
        {
            _shader.SetVec3("color", Color);

            base.Draw(comp);
        }

        public void SetColor(vec3 color)
            => Color = color;
        
    }
}
