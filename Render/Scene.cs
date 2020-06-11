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
        private FrameBuffer lightingFrameBuffer;

        private GL gl;

        public Scene()
        {
            gl = GL.GetApi();

            defaultShader = new Shader("Default/default");
            defaultShader.SetFloat("material.shininess", 32.0f);

            lightingShader = new Shader("Lighting/lightingShader", "Lighting/lightingShaderOrtho");

            var windowSize = Program.Window.GetSize();
            renderFrameBuffer = new FrameBuffer(windowSize.Width, windowSize.Height);
            renderFrameBuffer.CreateRenderBuffer((uint)windowSize.Width, (uint)windowSize.Height);
            Program.Window.OnResize += OnResize;

            lightingFrameBuffer = new FrameBuffer();
            lightingFrameBuffer.Use();
            gl.DrawBuffer(DrawBufferMode.None);
            gl.ReadBuffer(ReadBufferMode.None);

            Camera = new Camera();
            flashLight = new SpotLight(Camera.Position, Util.ToRadians(25), Util.ToRadians(30));
            //Lights.Add(flashLight);
            var sun = new DirectionalLight(new vec3(0.65854764f, -0.5150382f, -0.54868096f));
            sun.SetShadowsEnabled(true);
            Lights.Add(sun);
            Entities.Add(new Entity("Backpack/backpack.obj"));
        }

        public Texture Render()
        {
            Program.Settings.GetShadowMapSize(out int width, out int height);
            gl.Viewport(0, 0, (uint)width, (uint)height);
            lightingFrameBuffer.Use();

            foreach (Light l in Lights)
            {
                l.SetLightspaceMatrix(lightingFrameBuffer, lightingShader);
                Util.GLClear();
                RenderScene(lightingShader);
            }

            Program.Window.GetSize(out int windowWidth, out int windowHeight);
            gl.Viewport(0, 0, (uint)windowWidth, (uint)windowHeight);

            renderFrameBuffer.Use();
            Util.GLClear();
            gl.CullFace(CullFaceMode.Back);

            defaultShader.Use();
            Camera.SetData(defaultShader);
            setLightingData();
            RenderScene(defaultShader);

            return renderFrameBuffer.BoundTexture;
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
