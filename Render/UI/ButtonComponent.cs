using GlmSharp;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.UI
{
    abstract class ButtonComponent : QuadComponent
    {
        protected bool Hovered = false;

        public ButtonComponent(UILayer layer, vec2 size) : base(layer, size)
        { 
            Program.Window.OnCursorMove += OnMouseMove;
            Program.Window.OnMouseButton += OnMouseButton;
        }

        ~ButtonComponent() { Program.Window.OnMouseButton -= OnMouseButton; Program.Window.OnCursorMove -= OnMouseMove; }


        private void OnMouseMove(double x, double y, double deltaX, double deltaY)
        {
            vec2 mPos = new vec2((float)x, (float)y).ToUISpace();

            if (Transform.Orientation % Math.PI > float.Epsilon)
                mPos = Transform.Translation + (quat.FromAxisAngle(-Transform.Orientation, vec3.UnitZ) * new vec3(mPos - Transform.Translation, 0)).xy;

            var size = Quad.Size;

            var min = Transform.Translation - (size / 2);
            var max = Transform.Translation + (size / 2);

            Hovered = mPos.x >= min.x - 1 && mPos.x <= max.x + 1 && mPos.y >= min.y - 1 && mPos.y <= max.y + 1;
        }

        private void OnMouseButton(MouseButton button, InputAction action, KeyModifiers mods)
        {
            if (button == MouseButton.Left && Hovered)
            {
                if (action == InputAction.Press) OnClick();
                else if (action == InputAction.Release) OnSelect();
            }
                
        }

        protected abstract void OnSelect();
        protected abstract void OnClick();
    }
}
