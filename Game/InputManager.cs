using GlmSharp;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Main;

namespace Yahtzee.Game
{
    class InputManager
    {
        private List<Keys> Pressed = new List<Keys>();
        public vec2 MousePosition { get; protected set; } = vec2.Zero;

        public InputManager()
        {
            Program.Window.OnButton += OnButton;
            Program.Window.OnCursorMove += OnCursor;
        }

        private void OnCursor(double x, double y, double deltaX, double deltaY)        
            => MousePosition = new vec2((float)x, (float)y);
        

        private void OnButton(Keys key, InputAction action, KeyModifiers mods)
        {
            if (action == InputAction.Press)
                Pressed.Add(key);
            else if (action == InputAction.Release)
                Pressed.Remove(key);
        }

        public bool IsPressed(Keys key) => Pressed.Contains(key);
    }
}
