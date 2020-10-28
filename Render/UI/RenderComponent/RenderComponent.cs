using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Models;

namespace Yahtzee.Render.UI.RenderComponent
{
    abstract class RenderComponent
    {
        protected Shader _shader;


        public virtual void Draw(QuadComponent comp)
        {
            _shader.SetMat4("model", comp.Transform.ModelMatrixUI);
            _shader.SetVec2("screenSize", comp.Layer.UIFrameBuffer.BoundTexture.Size);
            comp.Quad.Draw(_shader);
        }

    }
}
