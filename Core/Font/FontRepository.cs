
using SharpFont;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Core.Font
{
    class FontRepository
    {

        private Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        private Library _lib;
        private GL _gl;

        public FontRepository(GL gl)
        {
            _gl = gl;
            _lib = new Library();
        }

        public Font GetFont(string fontName)
        {
            if (_fonts.ContainsKey(fontName))
                return _fonts.GetValueOrDefault(fontName);

            Font f = new Font(_gl, _lib, fontName);
            _fonts.Add(fontName, f);
            return f;
        }

        public bool DisposeFont(string fontName)
        {
            if (!_fonts.ContainsKey(fontName)) return false;
            _fonts.GetValueOrDefault(fontName).Dispose();
            _fonts.Remove(fontName);
            return true;
        }

    }
}
