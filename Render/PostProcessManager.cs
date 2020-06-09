using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;
using GlmSharp;
using Silk.NET.OpenGL;

namespace Yahtzee.Render
{
    class PostProcessManager
    {
        private ScreenMesh screenQuad;
        private static readonly float[] quadVertices = { 
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
             1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
             1.0f, -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f,  1.0f, 1.0f
        };

        private List<Shader> postProcessShaders = new List<Shader>();
        private Shader defaultShader;

        private FrameBuffer destintation, source, test;

        public PostProcessManager()
        {
            screenQuad = new ScreenMesh(quadVertices);

            System.Drawing.Size windowSize = Program.Window.GetSize();
            source = new FrameBuffer();
            destintation = new FrameBuffer(windowSize.Width, windowSize.Height);
            test = new FrameBuffer(windowSize.Width, windowSize.Height);

            defaultShader = new Shader("PostProcess/postPro", "PostProcess/postProDefault");
            defaultShader.SetInt("screen", 0);
        }

        public void RenderPostProcess(FrameBuffer renderBuffer)
        {
            if(postProcessShaders.Count <= 0)
            {
                FrameBuffer.UseDefault();
                Util.GLClear();
                renderBuffer.BoundTexture.BindToUnit(0);
                screenQuad.Draw(defaultShader);
                return;
            }

            source = renderBuffer;
            destintation = test;

            for(int i = 0; i < postProcessShaders.Count; i++)
            {
                Shader shader = postProcessShaders[i];
                bool lastShader = i >= postProcessShaders.Count - 1;

                if (!lastShader)
                    destintation.Use();
                else
                    FrameBuffer.UseDefault();

                Util.GLClear();
                source.BoundTexture.BindToUnit(0);
                screenQuad.Draw(shader);

                if (lastShader)
                    break;

                var temp = source;
                destintation = source;
                source = temp;
            }
        }

        public void AddPostProcessShader(string fragmentPath)
        {
            Shader shader = new Shader("PostProcess/postPro", "PostProcess/" + fragmentPath);
            postProcessShaders.Add(shader);
        }

        // I hate this solution, but it will do for now
        private unsafe class ScreenMesh
        {
            private Vertex2D[] Vertices;

            private GL gl;
            private uint VAO, VBO;

            public ScreenMesh(float[] data)
            {
                gl = GL.GetApi();

                Vertices = new Vertex2D[data.Length / 4];

                for(int i = 0; i < Vertices.Length; i++)
                {
                    Vertex2D v = new Vertex2D();
                    int di = i * 4;
                    v.Position = new vec2(data[di], data[di + 1]);
                    v.TexCoords = new vec2(data[di + 2], data[di + 3]);
                    Vertices[i] = v;
                }

                VAO = gl.GenVertexArray();
                gl.BindVertexArray(VAO);

                VBO = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

                fixed (void* i = &Vertices[0])
                    gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(Vertex2D)), i, BufferUsageARB.StaticDraw);

                gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex2D), (void*)0);
                gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vertex2D), (void*)sizeof(vec2));
                gl.EnableVertexAttribArray(0);
                gl.EnableVertexAttribArray(1);
            }

            public void Draw(Shader shader)
            {
                shader.Use();
                gl.BindVertexArray(VAO);
                gl.DrawArrays(GLEnum.Triangles, 0, (uint)Vertices.Length);
            }


            private struct Vertex2D
            {
                public vec2 Position;
                public vec2 TexCoords;
            }
        }

    }

}
