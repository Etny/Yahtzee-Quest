using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;
using GlmSharp;
using Silk.NET.OpenGL;
using Yahtzee.Render.Textures;
using Yahtzee.Core;
using Yahtzee.Render.Models;

namespace Yahtzee.Render
{
    class PostProcessManager : IRenderable
    {

        private List<Shader> postProcessShaders = new List<Shader>();
        private Shader defaultShader;

        private Texture texture1, texture2;
        private FrameBuffer postProBuffer;

        public PostProcessManager()
        {
            System.Drawing.Size windowSize = Program.Window.GetSize();
            texture1 = new Texture((uint)windowSize.Width, (uint)windowSize.Height);
            texture2 = new Texture((uint)windowSize.Width, (uint)windowSize.Height);
            postProBuffer = new FrameBuffer();

            defaultShader = ShaderRepository.GetShader("PostProcess/postPro", "PostProcess/postProDefault");
            defaultShader.SetInt("screen", 0);
        }

        public FrameBuffer Render(FrameBuffer renderFrameBuffer)
        {
            Texture renderTexture = renderFrameBuffer.BoundTexture;

            if(postProcessShaders.Count <= 0)  
                return renderFrameBuffer;
            

            var destination = texture2;
            var source = renderTexture;

            for(int i = 0; i < postProcessShaders.Count; i++)
            {
                Shader shader = postProcessShaders[i];
                bool lastShader = i >= postProcessShaders.Count - 1;

                if (!lastShader)
                {
                    postProBuffer.Use();
                    postProBuffer.BindTexture(destination);
                }
                else
                    renderFrameBuffer.Use();

                Util.GLClear();
                source.BindToUnit(0);
                QuadMesh.ScreenQuad.Draw(shader);

                if (lastShader)
                    break;

                if (i == 0)
                    source = texture1;

                var temp = destination;
                destination = source;
                source = temp;
            }

            return renderFrameBuffer;
        }

        public Shader AddPostProcessShader(string fragmentPath)
        {
            Shader shader = ShaderRepository.GetShader("PostProcess/postPro", "PostProcess/" + fragmentPath);
            shader.SetInt("screen", 0);
            postProcessShaders.Add(shader);
            return shader;
        }

        public void RemovePostProcessShader(Shader shader)
        {
            postProcessShaders.Remove(shader);
            shader.Dispose();
        }
        

    }

}
