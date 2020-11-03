using SharpFont;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Yahtzee.Render.Textures
{
    class GlyphTexture : ImageTexture
    {

        public unsafe GlyphTexture(GL gl, FTBitmap bitmap) : base()
        {
            TextureType = TextureType.Other;

            Size = new GlmSharp.uvec2((uint)bitmap.Width, (uint)bitmap.Rows);

            gl.PixelStore(GLEnum.UnpackAlignment, 1);

            gl.BindTexture(TextureTarget.Texture2D, ID);

            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);

            fixed (void* i = &bitmap.BufferData[0])
                gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Red, (uint)bitmap.Width, (uint)bitmap.Rows, 0, PixelFormat.Red, PixelType.UnsignedByte, i);

            gl.PixelStore(GLEnum.UnpackAlignment, 4);
        }

    }

}
