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
    unsafe class Texture : GLObject
    {

        private static uint[] boundTextures = new uint[16];

        public int BoundTextureUnit { get; protected set; } = -1;

        public Texture() : base() { ID = gl.GenTexture(); }

        public Texture(uint width, uint height) : this()
        {
            gl.BindTexture(TextureTarget.Texture2D, ID);
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, null);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void BindToUnit(int unit)
        {
            //if (boundTextures[unit] == ID)
            //    return;

            gl.ActiveTexture(TextureUnit.Texture0 + unit);
            Use();
            gl.ActiveTexture(TextureUnit.Texture30);
            gl.BindTexture(TextureTarget.Texture2D, 0);

            //boundTextures[unit] = ID;
        }

        public override void Use()
            => gl.BindTexture(TextureTarget.Texture2D, ID);

        public override void Dispose()
            => gl.DeleteTexture(ID);


    }

}
