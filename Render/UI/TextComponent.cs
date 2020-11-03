using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Core.Font;
using Yahtzee.Main;
using Yahtzee.Render.Models;

namespace Yahtzee.Render.UI
{
    class TextComponent : IUIComponent
    {

        public Transform2D Transform = Transform2D.Identity;

        protected UILayer _layer;

        public QuadMesh[] Quads { get; protected set; }
        private Glyph[] _glyphs;

        protected Shader _shader;

        public TextComponent(UILayer layer, string text)
        {
            _layer = layer;

            Quads = new QuadMesh[text.Length];
            _glyphs = new Glyph[text.Length];

            Font font = Program.FontRepository.GetFont("Resource/Fonts/arial.ttf");
            font.Size = 50;
            
            for(int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                Glyph g = font[c];
                QuadMesh q = new QuadMesh(g.Size);
                Console.WriteLine($"{g.Character}: {g.Bearing}");
                _glyphs[i] = g;
                Quads[i] = q;
            }


            _shader = ShaderRepository.GetShader("UI/UI", "UI/UIText");
        }
        
        public void Draw()
        {
            float advance = 0;

            _shader.SetVec2("screenSize", _layer.UIFrameBuffer.BoundTexture.Size);

            for (int i = 0; i < Quads.Length; i++)
            {
                Glyph g = _glyphs[i];

                if (g.Texture != null)
                {
                    Transform2D t = Transform;

                    t.Translation += new vec2(g.Bearing.x + advance, -(g.Size.y - g.Bearing.y));

                    _shader.SetMat4("model", t.ModelMatrixUI);
                    _shader.SetInt("glyph", 1);
                    g.Texture.BindToUnit(1);
                    Quads[i].Draw(_shader);
                }

                advance += g.Advance;
            }
        }

        public void Update(Time deltaTime)
        {
            
        }
    }
}
