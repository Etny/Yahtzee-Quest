using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Yahtzee.Core;
using Yahtzee.Core.Font;
using Yahtzee.Core.Physics;
using Yahtzee.Game.Scenes;
using Yahtzee.Render;

namespace Yahtzee.Main
{

    class Program
    {
        public static Window Window;
        public static Scene CurrentScene;
        public static InputManager InputManager;
        public static PostProcessManager PostProcessManager;
        public static Settings Settings;
        public static PhysicsManager PhysicsManager;
        public static Renderer Renderer;

        public static FontRepository FontRepository;

        private static GL gl;

        static void Main(string[] args)
        {
            InitSettings();

            Window = new Window();
            if (!Window.OpenWindow("Yahtzee Quest", new Size((int)Settings.CurrentScreenSize.x, (int)Settings.CurrentScreenSize.y)))
                return;
            gl = GL.GetApi();
            Window.SetVSync(true);
            Window.OnTick += Tick;
            Window.OnButton += OnButton;
            Window.OnResize += OnResize;

            SetupGL();

            Properties.Resources.ResourceManager.GetStream("postPro.vert");

            FontRepository = new FontRepository(gl);

            InputManager = new InputManager();
            PostProcessManager = new PostProcessManager();
            PhysicsManager = new PhysicsManager();
            CurrentScene = new SceneLoading();
            Renderer = new Renderer();

            CurrentScene.Init();
            PostProcessManager.AddPostProcessShader("gammaCorrect");

            Renderer.AddRenderable(CurrentScene);
            Renderer.AddRenderable(PostProcessManager);
            Renderer.AddRenderable(CurrentScene.UI);

            Window.StartLoop();

        }

        public static void SwitchScene(Scene newScene)
        {
            Renderer.RemoveRenderable(CurrentScene);
            Renderer.RemoveRenderable(CurrentScene.UI);

            CurrentScene = newScene;
            newScene.Init();

            Renderer.InsertRenderable(CurrentScene, 0);
            Renderer.InsertRenderable(CurrentScene.UI, 2);
        }

        private static void OnResize(int width, int height)
        {
            gl.Viewport(new Size(width, height));
        }

        private static void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Escape)
                Window.Close();
        }

        private static void Tick(Time deltaTime)
        { 
            CurrentScene.Update(deltaTime);

            Renderer.RenderPipeline();

            Window.EndRender();
        }

        private static void InitSettings()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string file = Path.Join(Path.GetDirectoryName(Path.GetDirectoryName(path)), "settings.txt");
            Console.WriteLine(file + " ||| " + File.Exists(file));
            int width = 1600, height = 900, sSize = 512;
                
            if(File.Exists(file))
            {
                Console.WriteLine("Found file!");
                string[] lines = File.ReadAllLines(file);
                
                foreach(string s in lines)
                {
                    string[] ss = s.Replace("\n","").ToLower().Split(":");
                    if (ss.Length != 2) continue;

                    switch (ss[0])
                    {
                        case "width":
                            width = int.Parse(ss[1]);
                            break;
                        case "height":
                            height = int.Parse(ss[1]);
                            break;
                        case "shadowsize":
                            sSize = int.Parse(ss[1]);
                            break;
                        default:
                            Console.WriteLine($"Can't parse setting \'{s}\'");
                            break;
                    }
                }
            }

            Settings = new Settings(width, height, sSize);
        }

        private static void SetupGL()
        {
            gl.Enable(EnableCap.CullFace);
            gl.Enable(EnableCap.DepthTest);
            gl.Enable(EnableCap.ProgramPointSize);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

    }
}
