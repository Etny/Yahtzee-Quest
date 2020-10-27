using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Yahtzee.Main;
using GlmSharp;
using Yahtzee.Core;
using Yahtzee.Render.Models;

namespace Yahtzee.Render.UI
{
    class UILayer : IRenderable
    {
        public ImmutableList<IUIComponent> Components;

        public FrameBuffer UIFrameBuffer { get; }
        private Shader _copyShader;

        public GL Gl;

        public UILayer()
        {
            Gl = GL.GetApi();

            Components = ImmutableList.Create<IUIComponent>();

            ivec2 UIWindowSize = Program.Window.GetSizeVec();
            UIFrameBuffer = new FrameBuffer(UIWindowSize.x, UIWindowSize.y);
            UIFrameBuffer.CreateRenderBuffer((uint)UIWindowSize.x, (uint)UIWindowSize.y);
            Program.Window.OnResize += OnResize;
            _copyShader = ShaderRepository.GetShader("PostProcess/postPro", "PostProcess/postProDefault");
            _copyShader.SetInt("screen", 0);

            Components = Components.Add(new ButtonComponent(this, "Resource/Images/UI/Buttons/Reroll.png"));
        }

        private void OnResize(int width, int height)
        {
            UIFrameBuffer.CreateTexture((uint)width, (uint)height);
            UIFrameBuffer.CreateRenderBuffer((uint)width, (uint)height);
        }


        public FrameBuffer Render(FrameBuffer frameBuffer)
        {
            UIFrameBuffer.Use();
            Util.GLClear();

            frameBuffer.BoundTexture.BindToUnit(0);


            Gl.DepthMask(false);
            QuadMesh.ScreenQuad.Draw(_copyShader);
            Gl.DepthMask(true);

            Components.ForEach(c => c.Draw());

            return UIFrameBuffer;
        }

        public void Update(Time deltaTime)
            => Components.ForEach(c => c.Update(deltaTime));
    }
}
