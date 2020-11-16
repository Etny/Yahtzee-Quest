using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Font;
using Yahtzee.Render.UI;

namespace Yahtzee.Game.SheetFields
{
    class SheetField
    {
        public TextComponent TextField { get; protected set; }

        private ScoreSheet _sheet;
        private vec2 _offset;
        private UILayer _layer;

        public Func<int[], int> Function;
        public bool Locked = false;
        public int Value = 0;
        public bool Lockable;

        public SheetField(ScoreSheet sheet, UILayer layer, Font font, Func<int[], int> func, bool lockable, vec2 offset)
        {
            _sheet = sheet;
            _offset = offset;
            Lockable = lockable;
            _layer = layer;
            Function = func;

            TextField = new TextComponent(layer, font, "0") { Alignment = TextAlignment.Centered, 
                                                            Color = Lockable ? new vec3(1, .6f, .05f) : new vec3(.05f, .7f, .2f) };
            layer.AddComponent(TextField);

            Update();
        }

        ~SheetField() { _layer.RemoveComponent(TextField); }

        public virtual void Update()
        {
            var halfSize = (_sheet.Quad.Size / 2 * _sheet.Transform.Scale);
            var topLeft = _sheet.Transform.Translation + new vec2(-halfSize.x, halfSize.y);

            TextField.Transform.Scale = _sheet.Transform.Scale / _sheet.maxScale;
            TextField.Transform.Translation = topLeft + (_sheet.Transform.Scale * (_sheet.smallSize * _offset).ScaleToScreen());
        }

        public void Lock()
        {
            if (!Lockable) return;
            Locked = true;
            TextField.Color = new vec3(0f, .2f, 1f);
        }

        public void UpdateText(int[] rolled)
        {
            if (Locked) return;
            int i = Function.Invoke(rolled);
            Value = i;
            TextField.Text = "" + i;
        }

    }
}
