using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Main
{
    class Util
    {

        public static float ToRadians(float deg)
           => (float)((deg / 180) * Math.PI);

        public static float ToDegrees(float rad)
            => (float)(rad / Math.PI) * 180;

        public static void GLClear()
            => GL.GetApi().Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
    }
}
