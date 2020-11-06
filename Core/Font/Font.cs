using System;
using System.Collections.Generic;
using System.Text;
using GlmSharp;
using SharpFont;
using Silk.NET.OpenGL;
using Yahtzee.Render.Textures;

namespace Yahtzee.Core.Font
{
    class Font : IDisposable
    {

        private readonly Face _face;
        private readonly Library _lib;
        private GL _gl;

        private Dictionary<char, Dictionary<float, Glyph>> _glyphs = new Dictionary<char, Dictionary<float, Glyph>>();

        public bool Disposed => _face.IsDisposed;

        public float Size { get => _size; set { _size = value; _face.SetCharSize(0, _size, 0, 96); } }
        private float _size = 0;


        public Font(GL gl, Library lib, string fontName)
        {
            _gl = gl;
            _lib = lib;
            _face = new Face(_lib, fontName);

            Size = 30;
        }

        public Glyph this[char c]
            => GetGlyph(c);

        public Glyph GetGlyph(char c)
        {
            if (Disposed) return null;

            if (!_glyphs.ContainsKey(c))
                _glyphs.Add(c, new Dictionary<float, Glyph>());

            if (!_glyphs[c].ContainsKey(_size))
                _glyphs[c].Add(_size, CreateGlyph(c));

            return _glyphs[c][_size];
        }

        private Glyph CreateGlyph(char c)
        {
            _face.LoadGlyph(_face.GetCharIndex(c), LoadFlags.Default, LoadTarget.Normal);
            var g = _face.Glyph;

            var bearing = new vec2(g.Metrics.HorizontalBearingX.ToSingle(), g.Metrics.HorizontalBearingY.ToSingle());
            var advance = g.Advance.X.ToSingle();
            var glyphSize = new vec2(g.Metrics.Width.ToSingle(), g.Metrics.Height.ToSingle());

            _face.Glyph.RenderGlyph(RenderMode.Normal);
            GlyphTexture texture = _face.Glyph.Bitmap.Width > 0 ? new GlyphTexture(_gl, _face.Glyph.Bitmap) : null;

            return new Glyph(c, _size, glyphSize, bearing, advance, texture);
        }

        public void Dispose()
        {
            _face.Dispose();

            _glyphs.Clear();
        }
    }
}
