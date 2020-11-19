using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;
using GlmSharp;
using Yahtzee.Render.Textures;
using Silk.NET.GLFW;
using Yahtzee.Game.Entities;
using Yahtzee.Render.Models;
using Yahtzee.Render.UI;
using Yahtzee.Core.Debug;
using Yahtzee.Core;

namespace Yahtzee.Render
{
    abstract class Scene : IRenderable
    {
        public List<Entity> Entities = new List<Entity>();
        public List<Light> Lights = new List<Light>();

        public Camera CurrentCamera;

        protected Shader DefaultShader;
        protected readonly GL Gl;

        private Shader _lightingShaderOrtho;
        private Shader _lightingShaderPersp;
        private FrameBuffer _lightingFrameBuffer;

        public UILayer UI;



        public Scene()
        {
            Gl = GL.GetApi();
        }
            
        ~Scene() { _lightingFrameBuffer.Dispose(); Program.Window.OnButton -= OnButton; }

        public virtual void Init()
        {
            Program.Settings.GetLightRange(out float near, out float far);
            Program.Window.GetSize(out int windowWidth, out int windowHeight);

            Program.Window.OnButton += OnButton;
            Program.Window.OnMouseButton += OnMouseButton;

            DefaultShader = ShaderRepository.GetShader("Default/default");
            DefaultShader.SetFloat("material.shininess", 32.0f);
            DefaultShader.SetFloat("lightNearPlane", near);
            DefaultShader.SetFloat("lightFarPlane", far);

            _lightingShaderOrtho = ShaderRepository.GetShader("Lighting/lightingShader", "Lighting/lightingShaderOrtho");
            _lightingShaderPersp = ShaderRepository.GetShader("Lighting/lightingShader", "Lighting/lightingShaderPersp");
            _lightingShaderPersp.SetFloat("farPlane", far);
            _lightingShaderOrtho.LightingShader = true;
            _lightingShaderPersp.LightingShader = true;

            _lightingFrameBuffer = new FrameBuffer();
            _lightingFrameBuffer.Use();
            Gl.DrawBuffer(DrawBufferMode.None);
            Gl.ReadBuffer(ReadBufferMode.None);
            FrameBuffer.UseDefault();

            CurrentCamera = new Camera();
            Entities.Add(CurrentCamera);

            UI = new UILayer();
        }

        public FrameBuffer Render(FrameBuffer renderFrameBuffer)
        {
            LightingPass();

            Program.Window.GetSize(out int windowWidth, out int windowHeight);
            Gl.Viewport(0, 0, (uint)windowWidth, (uint)windowHeight);

            renderFrameBuffer.Use();
            Gl.CullFace(CullFaceMode.Back);

            DefaultShader.Use();
            CurrentCamera.SetData(DefaultShader);
            SetLightingData();
            RenderScene(DefaultShader);
            RenderExtras(renderFrameBuffer);


            return renderFrameBuffer;
        }

        private void LightingPass()
        {
            if (!Lights.Exists(x => x.ShadowsEnabled))
                return;

            Gl.CullFace(CullFaceMode.Front);

            Program.Settings.GetShadowMapSize(out int width, out int height);
            Gl.Viewport(0, 0, (uint)width, (uint)height);
            _lightingFrameBuffer.Use();

            foreach (Light l in Lights.FindAll(x => x.ShadowsEnabled))
                l.CalculateShadows(_lightingFrameBuffer, l is PointLight ? _lightingShaderPersp : _lightingShaderOrtho, RenderScene);
        }

        private void SetLightingData()
        {
            int lights = 0;
            int shadowMapUnit = 0;
            
            foreach(Light light in Lights)
                light.SetValues(DefaultShader, lights++, ref shadowMapUnit);

            DefaultShader.SetInt("lightCount", lights);

            //This is to prevent a black screen on AMD hardware 
            if (!Lights.Exists(x => x is PointLight && x.ShadowsEnabled))
                DefaultShader.SetInt("shadowCube", 30);
            /*while(pointLights < Program.Settings.MaxLights)
                defaultShader.SetInt($"pointLights[{pointLights++}].shadowMap", 30);*/
        }

        public virtual void Update(Time deltaTime)
        { 
            Entities.ForEach(e => e.Update(deltaTime));
            Program.PhysicsManager.Update(deltaTime);
            UI.Update(deltaTime);
        }

        private void RenderScene(Shader shader)
        { 
            Entities.ForEach(e => { if (!(e is ModelEntity && (e as ModelEntity).DrawInstanced)) e.Draw(shader); });
            ModelManager.DrawModels(shader);
        }

        protected virtual void RenderExtras(FrameBuffer frameBuffer) { }

        protected virtual void OnButton(Keys key, InputAction action, KeyModifiers mods) { }

        protected virtual void OnMouseButton(MouseButton button, InputAction action, KeyModifiers mods) { }

    }
}
