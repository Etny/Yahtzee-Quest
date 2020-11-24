using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Curve;
using Yahtzee.Core.Font;
using Yahtzee.Main;
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
        public bool Selected = false;
        public bool Lockable;

        private float _currentValue { get{ return (int) (_valueCurve[_valueProgress] * Value); } }
        private float _targetValue = 0;
        private float _valueProgress = 0;
        private float _valueIncrease = .3f;
        private float _valueDurationPer10 = .4f;
        private static ICurve _valueCurve = new BezierCurve(new vec2(.42f, 0), new vec2(.58f, 1));

        private bool _visible = false;
        private float _currentAlpha = 0;
        private float _alphaDelta = 1.2f;


        private vec3 _color;

        public SheetField(ScoreSheet sheet, UILayer layer, Font font, Func<int[], int> func, bool lockable, vec2 offset)
        {
            _sheet = sheet;
            _offset = offset;
            Lockable = lockable;
            _layer = layer;
            Function = func;

            _color = Lockable ? new vec3(1, .6f, .05f) : new vec3(.05f, .7f, .2f);

            TextField = new TextComponent(layer, font, "0") { Alignment = TextAlignment.Centered,
                Color = _color};
            layer.AddComponent(TextField);

            CalcLocation();
        }

        ~SheetField() { _layer.RemoveComponent(TextField); }

        public virtual void Update(Time deltaTime)
        {
            CalcLocation();

            if (Locked)
                return;

            if ((int)_currentValue != _targetValue)
            {
                _valueProgress += (_targetValue > _currentValue ? 1 : -1) * deltaTime.DeltaF * _valueIncrease;
                if (_valueProgress > 1) _valueProgress = 1;
                if (_valueProgress < 0) _valueProgress = 0;
            }

            TextField.Text = "" + (int)_currentValue;

            if (!Lockable) return;

            if(_visible && _currentAlpha < 1)
                _currentAlpha = (float)Math.Min(_currentAlpha + (deltaTime.DeltaF * _alphaDelta), 1);
            else if (!_visible && _currentAlpha > 0)
                _currentAlpha = (float)Math.Min(_currentAlpha - (deltaTime.DeltaF * _alphaDelta), 1);

            TextField.Alpha = _currentAlpha;

            var color = _color;

            if (Selected)
            {
                var cos = ((float)(Math.Cos(deltaTime.Total * 4) + 1) / 4 ) + .5f;
                color = vec3.Lerp(_color, new vec3(0f, .2f, 1f), cos);
            }

            TextField.Color = color;

        }

        private void CalcLocation()
        {
            var halfSize = (_sheet.Quad.Size / 2 * _sheet.Transform.Scale);
            var topLeft = _sheet.Transform.Translation + new vec2(-halfSize.x, halfSize.y);

            TextField.Transform.Scale = _sheet.Transform.Scale / _sheet.maxScale;
            TextField.Transform.Translation = topLeft + (_sheet.Transform.Scale * (_sheet.smallSize * _offset).ScaleToScreen());
        }

        public void Clear()
        {
            if (Locked) return;
            _targetValue = 0;
            _visible = false;
        }

        public void Lock()
        {
            if (!Lockable) return;
            Locked = true;
            TextField.Text = "" + Value;
            TextField.Color = new vec3(0f, .2f, 1f);
            TextField.Alpha = 1;
        }

        public void UpdateText(int[] rolled)
        {
            if (Locked) return;
            int i = Function.Invoke(rolled);
            if (!Lockable && i != 0) _valueProgress = _targetValue / (float)i;
            Value = i;
            _targetValue = i;
            if (i != 0) _valueIncrease = 1 / (((float)i / 10f) * _valueDurationPer10);
            else _valueIncrease = .7f;
            _visible = true;
        }

    }
}
