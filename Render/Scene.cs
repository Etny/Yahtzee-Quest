using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Main;
using GlmSharp;
using Yahtzee.Render.Textures;
using Silk.NET.GLFW;

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
        private ModelEntity e;

        private Shader lightingShaderOrtho;
        private Shader lightingShaderPersp;
        private Shader defaultShader;

        private CollisionDetectionVisualizer PhysicsVisualizer;
        private PenetrationDepthVisualizer PenetrationDepthVisualizer = null;
        public ContactPointVisualizer ContactPointVisualizer = null;


        private FrameBuffer renderFrameBuffer;
        private FrameBuffer lightingFrameBuffer;

        private GL gl;
        int t = 0;

        public Scene()
        {
            gl = GL.GetApi();

            Program.Settings.GetLightRange(out float near, out float far);
            Program.Window.GetSize(out int windowWidth, out int windowHeight);

            Program.Window.OnButton += OnButton;

            defaultShader = ShaderRepository.GetShader("Default/default");
            defaultShader.SetFloat("material.shininess", 32.0f);
            defaultShader.SetFloat("lightNearPlane", near);
            defaultShader.SetFloat("lightFarPlane", far);

            lightingShaderOrtho = ShaderRepository.GetShader("Lighting/lightingShader", "Lighting/lightingShaderOrtho");
            lightingShaderPersp = ShaderRepository.GetShader("Lighting/lightingShader", "Lighting/lightingShaderPersp");
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
            //Lights.Add(flashLight);
            testLight = new DirectionalLight(new vec3(.1f, -.5f, -.5f));
            testLight.SetShadowsEnabled(true);
            Lights.Add(testLight);
        
            e = new ModelEntity("Basic/Cube.obj") { Position = new vec3(0f, -3f, 0) };
            e.Transform.Scale = new vec3(100, 1f, 100);
            e.RigidBody.Static = true;
            Entities.Add(e);
            Backpack = new ModelEntity("Basic/Cube.obj") { Position = new vec3(10f, -1f, 0)};
            Entities.Add(Backpack);

            PhysicsVisualizer = new CollisionDetectionVisualizer(Backpack, e, Program.PhysicsManager);
            ContactPointVisualizer = new ContactPointVisualizer(Program.PhysicsManager);
        }

        public Texture Render()
        {
            LightingPass();

            Program.Window.GetSize(out int windowWidth, out int windowHeight);
            gl.Viewport(0, 0, (uint)windowWidth, (uint)windowHeight);

            renderFrameBuffer.Use();
            gl.ClearFramebuffer();
            gl.CullFace(CullFaceMode.Back);

            defaultShader.Use();
            CurrentCamera.SetData(defaultShader);
            setLightingData();
            RenderScene(defaultShader);
            PhysicsVisualizer.Draw();
            PenetrationDepthVisualizer?.Draw();
            ContactPointVisualizer?.Draw();

            return renderFrameBuffer.BoundTexture;
        }

        private void LightingPass()
        {
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
            Program.PhysicsManager.Update(deltaTime);

            flashLight.SetPositionAndDirection(CurrentCamera.Position, CurrentCamera.GetDirection());
            
        }

        private void RenderScene(Shader shader)
            => Entities.ForEach(e => e.Draw(shader));

        private void OnResize(int width, int height)
        {
            renderFrameBuffer.CreateTexture((uint)width, (uint)height);
            renderFrameBuffer.CreateRenderBuffer((uint)width, (uint)height);
        }



        private void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            //if (action != InputAction.Press) return;

            if (key == Keys.ControlLeft && action == InputAction.Press)
                PhysicsVisualizer.UpdateGJK();

            else if (key == Keys.V && action == InputAction.Press)
                Backpack.Position += new vec3(0, 0.001f, 0);

            else if (key == Keys.B && action == InputAction.Press)
            {
                vec3 PenDepth = Program.PhysicsManager.DepthDetector.GetPenetrationDepth(Program.PhysicsManager.Collisions.GJKResult(Backpack.RigidBody, e.RigidBody));
                Backpack.Position -= PenDepth;
            }
            else if (key == Keys.C && action == InputAction.Press)
            {
                ContactPointVisualizer.SetResult(Program.PhysicsManager.Collisions.GJKResult(Backpack.RigidBody, e.RigidBody));
                ContactPointVisualizer.UpdateContactPoints();
            }
            else if (key == Keys.Apostrophe && action == InputAction.Press)
            {
                if (PenetrationDepthVisualizer == null)
                    PenetrationDepthVisualizer = new PenetrationDepthVisualizer(Program.PhysicsManager.Collisions.GJKResult(Backpack.RigidBody, e.RigidBody), Program.PhysicsManager);

                PenetrationDepthVisualizer.UpdateDepthTest();
            }
            else if (key == Keys.P && action == InputAction.Press)
            {
                vec3 o = new vec3(0, 4, 0);
                Random r = new Random();
                for(int i = 0; i < 3; i++)
                {
                    for(int j = 0; j < 3; j++)
                    {
                        ModelEntity m = new ModelEntity("Basic/Cube.obj") { Position = o + new vec3(i * 1.2f, 0, j * 1.2f) };
                        m.Transform.Rotate((float)r.NextDouble(), new vec3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));
                        Entities.Add(m);
                    }
                }
            }
        }
    }
}
