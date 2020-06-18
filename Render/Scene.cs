using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;
using GlmSharp;
using Yahtzee.Render.Textures;


namespace Yahtzee.Render
{
    class Scene
    {
        List<Entity> Entities = new List<Entity>();
        List<Light> Lights = new List<Light>();

        public Camera Camera;
        public SpotLight flashLight;
        public PointLight testLight;

        private Shader lightingShaderOrtho;
        private Shader lightingShaderPersp;
        private Shader defaultShader;

        private FrameBuffer renderFrameBuffer;
        private FrameBuffer lightingFrameBuffer;

        private GL gl;

        public Scene()
        {
            gl = GL.GetApi();

            Program.Settings.GetLightRange(out float near, out float far);
            Program.Window.GetSize(out int windowWidth, out int windowHeight);

            defaultShader = new Shader("Default/default");
            defaultShader.SetFloat("material.shininess", 32.0f);
            defaultShader.SetFloat("lightNearPlane", near);
            defaultShader.SetFloat("lightFarPlane", far);

            lightingShaderOrtho = new Shader("Lighting/lightingShader", "Lighting/lightingShaderOrtho");
            lightingShaderPersp = new Shader("Lighting/lightingShader", "Lighting/lightingShaderPersp");
            lightingShaderPersp.SetFloat("farPlane", far);

            renderFrameBuffer = new FrameBuffer(windowWidth, windowHeight);
            renderFrameBuffer.CreateRenderBuffer((uint)windowWidth, (uint)windowHeight);
            Program.Window.OnResize += OnResize;

            lightingFrameBuffer = new FrameBuffer();
            lightingFrameBuffer.Use();
            gl.DrawBuffer(DrawBufferMode.None);
            gl.ReadBuffer(ReadBufferMode.None);

            Camera = new Camera();
            //flashLight = new SpotLight(Camera.Position, Util.ToRadians(25), Util.ToRadians(30));
            flashLight = new SpotLight(new vec3(0, 3, -2), Util.ToRadians(25), Util.ToRadians(30)) { Direction = new vec3(0, -1, 1) };
            flashLight.SetShadowsEnabled(true);
            Lights.Add(flashLight);
            //var sun = new DirectionalLight(new vec3(0.65854764f, -0.5150382f, -0.54868096f));
            testLight = new PointLight(new vec3(0, 0, 1));
            testLight.SetShadowsEnabled(true);
            Lights.Add(testLight);
            Entities.Add(new Entity("Backpack/backpack.obj"));
        }

        public Texture Render()
        {
            LightingPass();

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

        private void LightingPass()
        {
            Program.Settings.GetShadowMapSize(out int width, out int height);
            gl.Viewport(0, 0, (uint)width, (uint)height);
            lightingFrameBuffer.Use();

            Shader lightingShader;

            foreach (Light l in Lights)
            {
                if (!l.ShadowsEnabled) continue;
                lightingShader = (l is PointLight) ? lightingShaderPersp : lightingShaderOrtho;
                l.CalculateShadows(lightingFrameBuffer, lightingShader, RenderScene);
            }
        }

        private void setLightingData()
        {
            int pointLights = 0, dirLights = 0, spotLights = 0;
            int shadowMapUnit = 8;
            
            foreach(Light light in Lights)
            {
                if (light is PointLight)
                    light.SetValues(defaultShader, pointLights++, ref shadowMapUnit);
                else if (light is DirectionalLight)
                    light.SetValues(defaultShader, dirLights++, ref shadowMapUnit);
                else if (light is SpotLight)
                    light.SetValues(defaultShader, spotLights++, ref shadowMapUnit);
            }

            defaultShader.SetInt("pointLightCount", pointLights);
            defaultShader.SetInt("dirLightCount", dirLights);
            defaultShader.SetInt("spotLightCount", spotLights);

            //This is to prevent a black screen on AMD hardware 
            while(pointLights < Program.Settings.MaxLights)
                defaultShader.SetInt($"pointLights[{pointLights++}].shadowMap", 30);
        }

        public void Update(Time deltaTime)
        {
            Camera.Update(deltaTime);
            //flashLight.SetPositionAndDirection(Camera.Position + new vec3(.5f, 0, 0), Camera.GetDirection());
            testLight.Position = new vec3(0, (float)(Math.Cos(deltaTime.Total) * 2), 1);
            Entities.ForEach(e => e.Update(deltaTime));
        }

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));

        private void OnResize(int width, int height)
            => renderFrameBuffer.CreateTexture((uint)width, (uint)height);
    }
}
