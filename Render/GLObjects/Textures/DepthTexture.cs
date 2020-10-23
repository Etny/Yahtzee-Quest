using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render.Textures
{
    unsafe class DepthTexture : Texture
    {
        public DepthTexture(int width, int height) : base()
        {
            gl.BindTexture(TextureTarget.Texture2D, ID);
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.DepthComponent, (uint)width, (uint)height, 0, PixelFormat.DepthComponent, PixelType.Float, null);
            
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            
            float[] borderColor = new float[] { 1, 1, 1, 1 };
            fixed (float* i = &borderColor[0])
                gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, i);
            
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }

    }
}
