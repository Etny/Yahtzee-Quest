﻿using Silk.NET.OpenGL;
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
        public List<Entity> Entities = new List<Entity>();
        List<Light> Lights = new List<Light>();

        public Camera CurrentCamera;
        public SpotLight flashLight;
        public DirectionalLight testLight;
        public PointLight testPointLight;
        private ModelEntity Backpack;

        private Shader lightingShaderOrtho;
        private Shader lightingShaderPersp;
        private Shader defaultShader;

        private FrameBuffer renderFrameBuffer;
        private FrameBuffer lightingFrameBuffer;

        private GL gl;

        bool lightingDone = false;

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

            CurrentCamera = new Camera();
            Entities.Add(CurrentCamera);
            flashLight = new SpotLight(new vec3(0, 3, -2), Util.ToRad(25), Util.ToRad(30)) { Direction = new vec3(0, -1, 1) };
            //flashLight.SetShadowsEnabled(true);
            Lights.Add(flashLight);
            //testLight = new DirectionalLight(new vec3(0, -1, -1));
            //testLight = new DirectionalLight(new vec3(0.65854764f, -0.5150382f, -0.54868096f));
            //testLight.SetShadowsEnabled(true);
            //Lights.Add(testLight);
            //testPointLight = new PointLight(vec3.Zero);
            //testPointLight.SetShadowsEnabled(true);
            //Lights.Add(testPointLight);

            ModelEntity e = new ModelEntity("Basic/Cube.obj") { Position = new vec3(0, -3, 0) };
            e.Transform.Scale = new vec3(10, 0.1f, 10);
            Entities.Add(e);
            Backpack = new ModelEntity("Backpack/backpack.obj");
            Entities.Add(Backpack);
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
            CurrentCamera.SetData(defaultShader);
            setLightingData();
            RenderScene(defaultShader);

            return renderFrameBuffer.BoundTexture;
        }

        private void LightingPass()
        {
            /*if (lightingDone) return;
            lightingDone = true;*/

            if (!Lights.Exists(x => x.ShadowsEnabled))
                return;

            gl.CullFace(CullFaceMode.Front);

            Program.Settings.GetShadowMapSize(out int width, out int height);
            gl.Viewport(0, 0, (uint)width, (uint)height);
            lightingFrameBuffer.Use();

            foreach (Light l in Lights.FindAll(x => x.ShadowsEnabled))
                l.CalculateShadows(lightingFrameBuffer, l is PointLight ? lightingShaderPersp : lightingShaderOrtho, RenderScene);
        }

        private void setLightingData()
        {
            int lights = 0;
            int shadowMapUnit = 0;
            
            foreach(Light light in Lights)
                light.SetValues(defaultShader, lights++, ref shadowMapUnit);

            defaultShader.SetInt("lightCount", lights);

            //This is to prevent a black screen on AMD hardware 
            if (!Lights.Exists(x => x is PointLight && x.ShadowsEnabled))
                defaultShader.SetInt("shadowCube", 30);
            /*while(pointLights < Program.Settings.MaxLights)
                defaultShader.SetInt($"pointLights[{pointLights++}].shadowMap", 30);*/
        }

        public void Update(Time deltaTime)
        { 
            Entities.ForEach(e => e.Update(deltaTime));
            flashLight.SetPositionAndDirection(CurrentCamera.Position, CurrentCamera.GetDirection());
            //testLight.Position = new vec3(0, (float)(Math.Cos(deltaTime.Total) * 2), 1);

            vec3 LightPos = new vec3((float)Math.Cos(deltaTime.Total) * 2, 2, (float)Math.Sin(deltaTime.Total) * 2);
            //testLight.Direction = (-LightPos).Normalized;
            //testPointLight.Position = LightPos;

            vec3 PackPos = new vec3(0, ((float)Math.Cos(deltaTime.Total) * 2) - 1, 0);
            Backpack.Position = PackPos;
        }

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));

        private void OnResize(int width, int height)
        {
            renderFrameBuffer.CreateTexture((uint)width, (uint)height);
            renderFrameBuffer.CreateRenderBuffer((uint)width, (uint)height);
        }
    }
}
