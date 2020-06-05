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


        public InputManager()
        {
            Program.Window.OnButton += OnButton;
        }

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
