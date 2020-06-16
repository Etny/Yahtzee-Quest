using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render.Textures
{
    unsafe class CubeMap : Texture
    {

        public CubeMap() : base() { }

        public CubeMap(int width, int height) : this(width, height, InternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte) { }

        public CubeMap(int width, int height, InternalFormat format, PixelFormat pixelFormat, PixelType pixelType) : this()
        {
            gl.BindTexture(TextureTarget.TextureCubeMap, ID);

            for (int i = 0; i < 6; i++)
                gl.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, (int)format,
                    (uint)width, (uint)height, 0, pixelFormat, pixelType, null);

            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }


    }
}
