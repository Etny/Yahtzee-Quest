using GlmSharp;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Yahtzee.Render;

namespace Yahtzee.Main
{
    static class Util
    {

        public static float ToRad(float deg)
           => (float)((deg / 180) * Math.PI);

        public static float ToDeg(float rad)
            => (float)(rad / Math.PI) * 180;

        public static float ToRad(double deg)
           => (float)((deg / 180) * Math.PI);

        public static float ToDeg(double rad)
            => (float)(rad / Math.PI) * 180;

        public static void GLClear()
            => GL.GetApi().Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
    
    
        public static vec3 GetClosetToOriginOnAffineHull(this vec3[] points)
        {
            return vec3.Zero;
        }

        private static vec3 ClosesToOriginTriangle(vec3[] points, bool clampToSimplex)
        {


            return vec3.NaN;
        }
    
    }
}
