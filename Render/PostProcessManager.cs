using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;
using GlmSharp;
using Silk.NET.OpenGL;
using Yahtzee.Render.Textures;
using Yahtzee.Render.UI;

namespace Yahtzee.Render
{
    class PostProcessManager
    {
        private QuadMesh screenQuad;

        private List<Shader> postProcessShaders = new List<Shader>();
        private Shader defaultShader;

        private Texture texture1, texture2;
        private FrameBuffer postProBuffer;

        public PostProcessManager()
        {
            screenQuad = new QuadMesh(1, 1);

            System.Drawing.Size windowSize = Program.Window.GetSize();
            texture1 = new Texture((uint)windowSize.Width, (uint)windowSize.Height);
            texture2 = new Texture((uint)windowSize.Width, (uint)windowSize.Height);
            postProBuffer = new FrameBuffer();

            defaultShader = ShaderRepository.GetShader("PostProcess/postPro", "PostProcess/postProDefault");
            defaultShader.SetInt("screen", 0);
        }

        public void RenderPostProcess(Texture renderTexture)
        {
            if(postProcessShaders.Count <= 0)
            {
                FrameBuffer.UseDefault();
                Util.GLClear();
                renderTexture.BindToUnit(0);
                screenQuad.Draw(defaultShader);
                return;
            }

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
                    FrameBuffer.UseDefault();

                Util.GLClear();
                source.BindToUnit(0);
                screenQuad.Draw(shader);

                if (lastShader)
                    break;

                if (i == 0)
                    source = texture1;

                var temp = destination;
                destination = source;
                source = temp;
            }
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
