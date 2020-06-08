using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;
using GlmSharp;

namespace Yahtzee.Render
{
    class Scene
    {
        List<Entity> Entities = new List<Entity>();
        List<Light> Lights = new List<Light>();

        public Camera Camera;
        public SpotLight flashLight;

        private Shader lightingShader;
        private Shader defaultShader;

        private FrameBuffer renderFrameBuffer;

        private GL gl;

        public Scene()
        {
            gl = GL.GetApi();

            defaultShader = new Shader("Default/default");
            defaultShader.SetFloat("material.shininess", 32.0f);

            var windowSize = Program.Window.GetSize();
            renderFrameBuffer = new FrameBuffer();
            renderFrameBuffer.CreateTexture((uint)windowSize.Width, (uint)windowSize.Height);
            Program.Window.OnResize += OnResize;

            Camera = new Camera();
            flashLight = new SpotLight(Camera.Position, Util.ToRadians(25), Util.ToRadians(30));
            Lights.Add(flashLight);
            Lights.Add(new DirectionalLight(new vec3(0.65854764f, -0.5150382f, -0.54868096f)));
            Entities.Add(new Entity("Backpack/backpack.obj"));
        }

        public void Render()
        {
            gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));

            gl.CullFace(CullFaceMode.Back);

            defaultShader.Use();
            Camera.SetData(defaultShader);
            setLightingData();
            RenderScene(defaultShader);
        }

        private void setLightingData()
        {
            int pointLights = -1, dirLights = -1, spotLights = -1;
            
            foreach(Light light in Lights)
            {
                if (light is PointLight)
                    light.SetValues(defaultShader, ++pointLights);
                else if (light is DirectionalLight)
                    light.SetValues(defaultShader, ++dirLights);
                else if (light is SpotLight)
                    light.SetValues(defaultShader, ++spotLights);
            }

            defaultShader.SetInt("pointLightCount", pointLights + 1);
            defaultShader.SetInt("dirLightCount", dirLights+1);
            defaultShader.SetInt("spotLightCount", spotLights+1);
        }

        public void Update(double deltaTime)
        {
            Camera.Update(deltaTime);
            flashLight.Position = Camera.Position;
            flashLight.Direction = Camera.GetDirection();
            Entities.ForEach(e => e.Update(deltaTime));
        }

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));

        private void OnResize(int width, int height)
            => renderFrameBuffer.CreateTexture((uint)width, (uint)height);
    }
}
