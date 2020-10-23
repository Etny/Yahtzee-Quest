using GlmSharp;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Yahtzee.Game;
using Yahtzee.Render;

namespace Yahtzee.Main
{
    static class Util
    {

        public static float ToRad(float deg)
           => (float)((deg / 180) * Math.PI);

        public static float ToDeg(float rad)
            => (float)(rad / Math.PI) * 180;

        public static vec3 ToRad(vec3 v)
            => new vec3(v.x.AsRad(), v.y.AsRad(), v.z.AsRad());

        public static vec3 ToDeg(vec3 v)
            => new vec3(v.x.AsDeg(), v.y.AsDeg(), v.z.AsDeg());

        public static vec3 asRad(this vec3 v)
            => new vec3(v.x.AsRad(), v.y.AsRad(), v.z.AsRad());

        public static vec3 asDeg(this vec3 v)
            => new vec3(v.x.AsDeg(), v.y.AsDeg(), v.z.AsDeg());

        public static float AsRad(this float deg)
           => (float)((deg / 180) * Math.PI);

        public static float AsDeg(this float rad)
            => (float)(rad / Math.PI) * 180;

        public static float ToRad(double deg)
           => (float)((deg / 180) * Math.PI);

        public static float ToDeg(double rad)
            => (float)(rad / Math.PI) * 180;

        public static void GLClear()
            => GL.GetApi().Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));
    
        public static void ClearFramebuffer(this GL gl)
            => gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit));

        public static vec2 WorldSpaceToScreenSpace(Transform transform, Camera camera)
            => ToScreenSpace(vec3.Zero, transform, camera);

        public static vec2 ToScreenSpace(this vec3 worldSpace, Transform transform, Camera camera)
            => WorldSpaceToScreenSpace(worldSpace, transform, camera);

        public static vec2 WorldSpaceToScreenSpace(vec3 worldSpace, Transform transform, Camera camera)
        {
            var v1 = (camera.ProjectionMatrix * camera.ViewMatrix * transform.ModelMatrix * new vec4(worldSpace, 1));
            var v2 = v1.xy / v1.w;
            v2 = (v2 + vec2.Ones) / 2;
            Program.Window.GetSize(out int screenW, out int screenH);
            v2 *= new vec2(screenW, screenH);

            return v2;
        }

        public static vec2 ToNDC(this vec2 v)
        {
            Program.Window.GetSize(out int width, out int height);

            v /= new vec2(width, height);
            v = (v * 2) - vec2.Ones;

            return v;
        }
    }
}
