using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Font;
using Yahtzee.Main;
using Yahtzee.Render.Models;
using System.Linq;

namespace Yahtzee.Render.UI
{
    class TextComponent : IUIComponent
    {

        public Transform2D Transform = Transform2D.Identity;

        protected UILayer _layer;

        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public Font Font { get; protected set; }
        public int Lines { get; protected set; }

        public string Text { get => _text; set => SetText(value); }
        private string _text;

        public vec3 Color { get; set; } = vec3.Ones;

        private QuadMesh _quad;
        private Glyph[][] _glyphs;
        private float[] lineAdvance;

        protected Shader _shader;

        private static float _newlinePadding = 5;
        private float _newlineSpacing;

        public TextComponent(UILayer layer, Font font)
        {
            _layer = layer;

            Font = font;
            _newlineSpacing = (Font['g'].Size.y - Font['g'].Bearing.y) + Font['T'].Bearing.y + _newlinePadding;

            _quad = new QuadMesh(vec2.Ones);
            _shader = ShaderRepository.GetShader("UI/UI", "UI/UIText");
        }

        public TextComponent(UILayer layer, Font f, string text) : this(layer, f) { SetText(text); }
        public TextComponent(UILayer layer) : this(layer, Program.FontRepository.GetFont("arial.ttf")) { }
        public TextComponent(UILayer layer, string text) : this(layer, Program.FontRepository.GetFont("arial.ttf"), text) { }

        private void SetText(string text)
        {
            string trimmed = new string((
               from a in text where
               char.IsLetterOrDigit(a) || char.IsWhiteSpace(a) || char.IsPunctuation(a) || a == '\n'
               select a).ToArray());

            _text = trimmed;

            string[] lines = trimmed.Split('\n');
            Lines = lines.Length;

            _glyphs = new Glyph[Lines][];
            lineAdvance = new float[Lines];

            for (int j = 0; j < Lines; j++)
            {
                _glyphs[j] = new Glyph[lines[j].Length];

                for (int i = 0; i < lines[j].Length; i++)
                {
                    char c = lines[j][i];

                    Glyph g = Font[c];
                    _glyphs[j][i] = g;
                    lineAdvance[j] += g.Advance;
                }
            }

        }

        public void Draw()
        {
            _layer.Gl.Enable(Silk.NET.OpenGL.EnableCap.Blend);

            float advanceY = 0;
            var scale = Transform.Scale;
            _shader.SetVec2("screenSize", _layer.UIFrameBuffer.BoundTexture.Size);
            _shader.SetVec3("color", Color);

            for (int j = 0; j < Lines; j++)
            {
                float advanceX = 0;

                if (Alignment == TextAlignment.Centered)
                    advanceX -= lineAdvance[j]/ 2;
                else if (Alignment == TextAlignment.Right)
                    advanceX -= lineAdvance[j];


                for (int i = 0; i < _glyphs[j].Length; i++)
                {
                    Glyph g = _glyphs[j][i];

                    if (g.Texture != null)
                    {
                        Transform2D t = Transform;


                        t.Scale = scale * g.Size;
                        t.Translation += scale * new vec2(g.Bearing.x + advanceX + (g.Size.x / 2), -(g.Size.y / 2) + g.Bearing.y + advanceY);

                        _shader.SetMat4("model", t.ModelMatrixUI);
                        _shader.SetInt("glyph", 1);
                        g.Texture.BindToUnit(1);
                        _quad.Draw(_shader);
                    }

                    advanceX += g.Advance;
                }

                advanceY -= _newlineSpacing;
            }

            _layer.Gl.Disable(Silk.NET.OpenGL.EnableCap.Blend);
        }

        public void Update(Time deltaTime)
        {
            
        }
    }

    public enum TextAlignment
    {
        Left, Right, Centered
    }
}
