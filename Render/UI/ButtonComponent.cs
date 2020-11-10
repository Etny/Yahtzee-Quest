using GlmSharp;
using Silk.NET.GLFW;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using Yahtzee.Render.Textures;
using Yahtzee.Render.UI.RenderComponent;

namespace Yahtzee.Render.UI
{
    class ButtonComponent : QuadComponent
    {
        public bool Hovered { get; protected set; } = false;

        public event EventHandler OnClick;
        public event EventHandler OnRelease;
        public event EventHandler OnHover;

        public ButtonComponent(UILayer layer, vec2 size, RenderComponent.RenderComponent component) : base(layer, size, component)
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

            var size = Transform.Scale * Quad.Size;

            var min = Transform.Translation - (size / 2);
            var max = Transform.Translation + (size / 2);

            bool hovered = mPos.x >= min.x - 1 && mPos.x <= max.x + 1 && mPos.y >= min.y - 1 && mPos.y <= max.y + 1;

            if (!Hovered && hovered) OnHover?.Invoke(this, null);

            Hovered = hovered;
        }

        private void OnMouseButton(MouseButton button, InputAction action, KeyModifiers mods)
        {
            if (button == MouseButton.Left && Hovered)
            {
                if (action == InputAction.Press) OnClick?.Invoke(this, null);
                else if (action == InputAction.Release) OnRelease?.Invoke(this, null);
            }

        }

    }

}
