using GlmSharp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game.Entities;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game
{
    class Outliner : IDisposable
    {

        public ModelEntity Entity = null;
        public bool Enabled = false;

        public vec3 Color = new vec3(1);
        public float OutlineSize = .1f;

        private Shader _shader;
        private GL _gl;

        public Outliner(GL gl, ModelEntity e = null)
        {
            _gl = gl;
            Entity = e;

            _shader = new Shader("Default/default", "PostProcess/postProColor");
        }

        ~Outliner() => Dispose();

        public void Draw()
        {
            if (Entity == null || !Enabled) return;


            _gl.Enable(EnableCap.StencilTest);
            _gl.Disable(EnableCap.DepthTest);
            _gl.DepthMask(false);
            _gl.StencilMask(0xff);
            _gl.Clear((uint)ClearBufferMask.StencilBufferBit);

            _gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            _gl.StencilFunc(StencilFunction.Always, 1, 0xff);
            _gl.ColorMask(false, false, false, false);

            Entity.Draw(_shader);

            //_gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
            _gl.StencilFunc(StencilFunction.Notequal, 1, 0xff);
            _gl.StencilMask(0x00);
            _gl.ColorMask(true, true, true, true);

            var s = Entity.Transform.Scale;
            Entity.Transform.Scale = s * (1 + OutlineSize);
            Entity.Draw(_shader);
            Entity.Transform.Scale = s;

            _gl.StencilFunc(StencilFunction.Always, 1, 0xff);
            _gl.Disable(GLEnum.StencilTest);
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthMask(true);
        }

        public void Dispose()
            => _shader.Dispose();

    }
}
