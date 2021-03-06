﻿using GlmSharp;
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

        private bool firstMove = true;
        private double lastX = -1, lastY = -1;

        public event Action<Time> OnTick;
        public event Action<int, int> OnResize;
        public event Action<double, double, double, double> OnCursorMove;
        public event Action<Keys, InputAction, KeyModifiers> OnButton;
        public event Action<MouseButton, InputAction, KeyModifiers> OnMouseButton;

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
            glfw.SetWindowSizeLimits(window, dimensions.Width, dimensions.Height, dimensions.Width, dimensions.Height);
            //glfw.SetWindowSizeCallback(window, OnWindowResize);
            glfw.SetKeyCallback(window, OnWindowButtonPress);
            glfw.SetCursorEnterCallback(window, OnWindowCursorEnter);
            glfw.SetCursorPosCallback(window, OnWindowCursor);
            glfw.SetMouseButtonCallback(window, OnWindowMouseButton);
            //glfw.SetScrollCallback(window, ScrollInput);
            //glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);

            return true;
        }

        private void OnWindowMouseButton(WindowHandle* window, MouseButton button, InputAction action, KeyModifiers mods)
            => OnMouseButton?.Invoke(button, action, mods);            

        private void OnWindowCursorEnter(WindowHandle* window, bool entered)
        {
            if (!entered) firstMove = true; ;
        }

        private void OnWindowCursor(WindowHandle* window, double x, double y)
        {
            if (!firstMove)
                OnCursorMove?.Invoke(x, y, x - lastX, y - lastY);
            else
                firstMove = false;
            
            lastX = x;
            lastY = y;  
        }

        private void OnWindowButtonPress(WindowHandle* window, Keys key, int scanCode, InputAction action, KeyModifiers mods)
            => OnButton?.Invoke(key, action, mods);

        private void OnWindowResize(WindowHandle* window, int width, int height)
            => OnResize?.Invoke(width, height);
        

        public void StartLoop()
        {
            lastFrame = glfw.GetTimerValue();
            long step = 0;

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();
                currentFrame = glfw.GetTimerValue();
                deltaTime = (currentFrame - lastFrame) / glfw.GetTimerFrequency();
                lastFrame = currentFrame;

                //Console.WriteLine($"FPS: {1f / deltaTime}");
                if (deltaTime > (1f / 30f)) deltaTime = 1f / 30f;

                if (OnTick == null) break;
                OnTick.Invoke(new Time(deltaTime, glfw.GetTime(), step));
                step++;
            }

            glfw.Terminate();
        }

        public void EndRender()
        {
            glfw.SwapBuffers(window);
        }

        public void SetVSync(bool vsync)
        {
            glfw.SwapInterval(vsync ? 1 : 0);
        }

        public void Close()
            => glfw.SetWindowShouldClose(window, true);


        public void GetSize(out int width, out int height)
        {
            glfw.GetWindowSize(window, out width, out height);
        }

        public ivec2 GetSizeVec()
        {
            glfw.GetWindowSize(window, out int width, out int height);
            return new ivec2(width, height);
        }
            

        public Size GetSize()
        {
            glfw.GetWindowSize(window, out int width, out int height);
            return new Size(width, height);
        }

        private static void GlfwError(Silk.NET.GLFW.ErrorCode error, string msg)
            => Console.WriteLine($"Glfw encountered an error (code {error}): {msg}");
        
    }

    class Time
    {
        public readonly double Delta;
        public readonly double Total;
        public readonly long Step;
        public float DeltaF { get { return (float)Delta; } }

        public Time(double delta, double total, long step) { Delta = delta; Total = total; Step = step; }
    }
}
