using GlmSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Textures;

namespace Yahtzee.Core.Font
{
    class Glyph
    {
        public readonly char Character;
        public readonly float TextSize;
        public readonly vec2 Size;
        public readonly vec2 Bearing;
        public readonly float Advance;
        public readonly GlyphTexture Texture;

        public Glyph(char character, float textSize, vec2 size, vec2 bearing, float advance, GlyphTexture texture)
        {
            Character = character;
            TextSize = textSize;
            Size = size;
            Bearing = bearing;
            Advance = advance;
            Texture = texture;
        }

    }
}
