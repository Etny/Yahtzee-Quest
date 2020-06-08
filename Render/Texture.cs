using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Yahtzee.Render
{
    unsafe class Texture : GLObject
    {
        public Texture() : base()
        {
            ID = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, ID);
        }

        public Texture(uint width, uint height) : this()
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, null);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void BindToUnit(int unit)
        {
            gl.ActiveTexture(TextureUnit.Texture0 + unit);
            Use();
        }

        public override void Use()
            => gl.BindTexture(TextureTarget.Texture2D, ID);

        public override void Dispose()
            => gl.DeleteTexture(ID);


    }

}
