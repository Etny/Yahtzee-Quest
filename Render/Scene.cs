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

            Program.Settings.GetLightRange(out float near, out float far);
            Program.Window.GetSize(out int windowWidth, out int windowHeight);

            defaultShader = new Shader("Default/default");
            defaultShader.SetFloat("material.shininess", 32.0f);
            defaultShader.SetFloat("lightNearPlane", near);
            defaultShader.SetFloat("lightFarPlane", far);

            lightingShader = new Shader("Lighting/lightingShader", "Lighting/lightingShaderOrtho");

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
            SpotLight sun = new SpotLight(Camera.Position, Util.ToRadians(15), Util.ToRadians(20)) { Direction = Camera.GetDirection() };
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
                if (!l.ShadowsEnabled) continue;
                //lightingShader.SetFloat("farPlane", 50);
                //lightingShader.SetVec3("lightPos", ((SpotLight)l).Position);
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

            //Console.WriteLine(spotLights);
        }

        public void Update(double deltaTime)
        {
            Camera.Update(deltaTime);
            //flashLight.SetPositionAndDirection(Camera.Position + new vec3(.5f, 0, 0), Camera.GetDirection());
            Entities.ForEach(e => e.Update(deltaTime));
        }

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));

        private void OnResize(int width, int height)
            => renderFrameBuffer.CreateTexture((uint)width, (uint)height);
    }
}
