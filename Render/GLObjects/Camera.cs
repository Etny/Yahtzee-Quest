using GlmSharp;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using Yahtzee.Core;
using Yahtzee.Game;
using Yahtzee.Game.Entities;
using Yahtzee.Main;

namespace Yahtzee.Render
{
    unsafe class Camera : Entity
    {
        public vec3 Front = new vec3(0.0f, 0.0f, -1.0f);
        public vec3 Up = new vec3(0.0f, 1.0f, 0.0f);

        public float Pitch = 0.0f;
        public float Yaw = 0.0f;

        private GL gl;
        private uint matricesBuffer;

        public readonly float Fov = 90;
        public readonly float NearZ = .1f;
        public readonly float FarZ = 1000f;

        public mat4 ProjectionMatrix { get; protected set; }
        public mat4 ViewMatrix { get; protected set; }

        public Camera()
        {
            gl = GL.GetApi();

            Program.Window.OnResize += OnResize;
            //Program.Window.OnCursorMove += OnCursorMove;

            MovementController = new MovementControllerWASD(2.5f, GetDirection);

            Position = new vec3(0, 0, 3);

            matricesBuffer = gl.CreateBuffer();
            gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesBuffer);
            gl.BufferData(BufferTargetARB.UniformBuffer, (uint)(3 * sizeof(mat4)), null, BufferUsageARB.StaticDraw);
            gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, matricesBuffer);

            Size windowSize = Program.Window.GetSize();
            float aspectRatio = (float)windowSize.Height / (float)windowSize.Width;
            float orthoSize = 5f;
            mat4 orthoMatrix = mat4.Ortho(-orthoSize, orthoSize, -orthoSize * aspectRatio, orthoSize * aspectRatio, NearZ, FarZ);
            ProjectionMatrix = mat4.PerspectiveFov(Util.ToRad(Fov), windowSize.Width, windowSize.Height, NearZ, FarZ);
            var pm = ProjectionMatrix;
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(mat4), &pm);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(mat4), (uint)sizeof(mat4), &orthoMatrix);
        }

        public mat4 LookAt() => mat4.LookAt(Position, Position + GetDirection(), Up);
        public vec3 GetDirection() => (Transform.Orientation * Front).NormalizedSafe;

        public void SetData(Shader shader)
        {
            ViewMatrix = LookAt();
            var viewMat = ViewMatrix;
            gl.BindBuffer(BufferTargetARB.UniformBuffer, matricesBuffer);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 2 * sizeof(mat4), (uint)sizeof(mat4), &viewMat);
            shader.SetVec3("viewPos", Position);
        }

        public vec3 GetMouseRay()
        {
            var mPos = Program.InputManager.MousePosition;
            var screenSize = Program.Window.GetSize();
            float aspectRatio = (float)screenSize.Width / (float)screenSize.Height;
            var screenPos = new vec2(mPos.x / screenSize.Width, mPos.y / screenSize.Height);
            var nearPlaneSize = NearZ * (float)(Math.Tan(Fov.AsRad() / 2));

            vec3 worldSpace = new vec3((2 * screenPos.x - 1) * aspectRatio * nearPlaneSize, (1 - 2 * screenPos.y) * nearPlaneSize, -NearZ);
            worldSpace = Transform * worldSpace;

            return (worldSpace - Position).NormalizedSafe;
        }


        private void OnCursorMove(double x, double y, double deltaX, double deltaY)
        {
            float sensitivity = 0.2f;
            deltaX *= -sensitivity;
            deltaY *= -sensitivity;

            Yaw += (float)deltaX;
            Pitch += (float)deltaY;

            if (Pitch > 89f)
                Pitch = 89f;
            if (Pitch < -89f)
                Pitch = -89f;

            Transform.Orientation = quat.FromAxisAngle(Util.ToRad(Yaw), vec3.UnitY) * quat.FromAxisAngle(Util.ToRad(Pitch), vec3.UnitX);
        }

        private void OnResize(int width, int height)
        {
            mat4 projectionMatrix = mat4.PerspectiveFov(Fov, width, height, .1f, 1000f);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(mat4), &projectionMatrix);
        }


    }
}