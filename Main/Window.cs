using Silk.NET.Core.Loader;
using Silk.NET.Core.Platform;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Yahtzee.Main
{
    unsafe class Window
    { 
        private Glfw glfw;

        private WindowHandle* window;

        public delegate void Tick(double deltaTime);
        public event Tick OnTick;

        private double lastFrame = 0.0f;
        private double deltaTime = 0.0f;
        private double currentFrame = 0.0f;

        public bool OpenWindow(string name, Size dimensions)
        {
            glfw = Glfw.GetApi();

            glfw.SetErrorCallback(GlfwError);

            SilkManager.Register<GLSymbolLoader>(new Silk.NET.GLFW.GlfwLoader());

            glfw.Init();
            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

            window = glfw.CreateWindow(dimensions.Width, dimensions.Height, name, null, null);

            if (window == null)
            {
                Console.WriteLine("Window creation failed");
                glfw.Terminate();
                return false;
            }


            glfw.MakeContextCurrent(window);
            //glfw.SetWindowSizeCallback(window, OnResize);
            //glfw.SetCursorPosCallback(window, MouseInput);
            //glfw.SetScrollCallback(window, ScrollInput);
            glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);

            return true;
        }

        public void StartLoop()
        {
            lastFrame = glfw.GetTimerValue();

            while (!glfw.WindowShouldClose(window))
            {
                currentFrame = glfw.GetTimerValue();
                deltaTime = (currentFrame - lastFrame) / glfw.GetTimerFrequency();
                lastFrame = currentFrame;

                if (OnTick == null) break;
                OnTick.Invoke(deltaTime);
            }

            glfw.Terminate();
        }

        public void EndRender()
        {
            glfw.SwapBuffers(window);
            glfw.PollEvents();
        }


        public void Close()
            => glfw.SetWindowShouldClose(window, true);
        

        public Size GetSize()
        {
            glfw.GetWindowSize(window, out int width, out int height);
            return new Size(width, height);
        }

        private static void GlfwError(Silk.NET.GLFW.ErrorCode error, string msg)
            => Console.WriteLine($"Glfw encountered an error (code {error}): {msg}");
        


    }
}
