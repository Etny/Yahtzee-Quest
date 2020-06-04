using GlmSharp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using Yahtzee.Main;

namespace Yahtzee.Render
{
    unsafe class Camera
    {
        public vec3 Position = new vec3(0.0f, 0.0f, 3.0f);
        public vec3 Front = new vec3(0.0f, 0.0f, -1.0f);
        public vec3 Up = new vec3(0.0f, 1.0f, 0.0f);

        public float Pitch = 0.0f;
        public float Yaw = -90.0f;

        private GL gl;
        private uint matricesBuffer;

        private float Fov = 90;

        public Camera()
        {
            gl = GL.GetApi();

            Program.Window.OnResize += OnResize;

            matricesBuffer = gl.CreateBuffer();
            gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesBuffer);
            gl.BufferData(BufferTargetARB.UniformBuffer, (uint)(2 * sizeof(mat4)), null, BufferUsageARB.StaticDraw);
            gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, matricesBuffer);

            Size windowSize = Program.Window.GetSize();
            mat4 projectionMatrix = mat4.PerspectiveFov(Fov, windowSize.Width, windowSize.Height, .1f, 1000f);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(mat4), &projectionMatrix);
        }

        public mat4 LookAt() => mat4.LookAt(Position, Position + GetDirection(), Up);
        public vec3 GetDirection()
        {
            float x = (float)(Math.Cos(Util.ToRadians(Yaw)) * Math.Cos(Util.ToRadians(Pitch)));
            float y = (float)Math.Sin(Util.ToRadians(Pitch));
            float z = (float)(Math.Sin(Util.ToRadians(Yaw)) * Math.Cos(Util.ToRadians(Pitch)));
            return new vec3(x, y, z).Normalized;
        }

        public void SetMatrices()
        {
            mat4 viewMat = LookAt();
            gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesBuffer);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(mat4), (uint)sizeof(mat4), &viewMat);
        }

        public void SetDirection(vec3 dir)
        {
            Yaw = Util.ToDegrees((float)Math.Atan2(dir.y, dir.x));
            Pitch = Util.ToDegrees((float)-Math.Asin(dir.x));
        }


        private void OnResize(int width, int height)
        {
            mat4 projectionMatrix = mat4.PerspectiveFov(Fov, width, height, .1f, 1000f);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(mat4), &projectionMatrix);
        }


    }
}
