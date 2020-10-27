using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Main;
using Yahtzee.Render.Models;

namespace Yahtzee.Render
{
    class Renderer
    {
        public ImmutableList<IRenderable> Pipeline { get; protected set; }

        public FrameBuffer RenderFrameBuffer { get; }

        private Shader _finalShader;

        public Renderer()
        {
            Pipeline = ImmutableList<IRenderable>.Empty;

            Program.Window.GetSize(out int windowWidth, out int windowHeight);
            RenderFrameBuffer = new FrameBuffer(windowWidth, windowHeight);
            RenderFrameBuffer.CreateRenderBuffer((uint)windowWidth, (uint)windowHeight);
            Program.Window.OnResize += OnResize;

            _finalShader = ShaderRepository.GetShader("PostProcess/postPro", "PostProcess/postProDefault");
        }

        private void OnResize(int width, int height)
        {
            RenderFrameBuffer.CreateTexture((uint)width, (uint)height);
            RenderFrameBuffer.CreateRenderBuffer((uint)width, (uint)height);
        }

        public void RenderPipeline()
        {
            FrameBuffer buffer = RenderFrameBuffer;

            buffer.Use();
            GL.GetApi().ClearFramebuffer();

            for (int i = 0; i < Pipeline.Count; i++)
                buffer = Pipeline[i].Render(buffer);

            buffer.BoundTexture.BindToUnit(0);
            FrameBuffer.UseDefault();
            Util.GLClear();
            _finalShader.SetInt("screen", 0);
            QuadMesh.ScreenQuad.Draw(_finalShader);
        }

        public void InsertRenderable(IRenderable r, int inder)
            => Pipeline = Pipeline.Insert(inder, r);

        public void AddRenderable(IRenderable r)
            => Pipeline = Pipeline.Add(r);

        public void RemoveRenderable(IRenderable r)
        {
            if (Pipeline.Contains(r) && Pipeline.Count > 1) Pipeline = Pipeline.Remove(r);
        }
    }
}
