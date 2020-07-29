using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render;
using GlmSharp;
using Silk.NET.OpenGL;

namespace Yahtzee.Game.Debug
{
    unsafe class LineMesh : Mesh
    {

        private Shader shader;

        private int MaxPoints = 4;

        public vec3[] Points = new vec3[0];
        public vec3[] Colors = new vec3[0];

        public int LineWidth;

        public LineMesh(vec3[] points = null, vec3[] colors = null, int lineWidth = 5) : base()
        {
            if (points != null) Points = points;
            if (colors != null) Colors = colors;

            LineWidth = lineWidth;

            shader = new Shader("Debug/Visualizer/vis");

            setupMesh();

            if (points != null) SetPoints(Points);
            if (colors != null) SetColors(Colors);
        }

        protected override void setupMesh()
        {
            VBO = gl.GenBuffer();
            VAO = gl.GenVertexArray();

            gl.BindVertexArray(VAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

            gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(sizeof(vec3) * (MaxPoints * 2)), null, BufferUsageARB.DynamicDraw);

            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(vec3), (void*)(sizeof(vec3) * MaxPoints));
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, (uint)sizeof(vec3), (void*)0);
            gl.EnableVertexAttribArray(0);
            gl.EnableVertexAttribArray(1);
        }

        public void SetColors(vec3[] Colors)
        {
            this.Colors = Colors;

            gl.BindVertexArray(VAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

            fixed (void* i = &Colors[0])
                gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (uint)(sizeof(vec3) * Colors.Length), i);
        }

        public void SetPoints(vec3[] Points)
        {
            this.Points = Points;

            gl.BindVertexArray(VAO);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);

            fixed (void* i = &Points[0])
                gl.BufferSubData(BufferTargetARB.ArrayBuffer, (sizeof(vec3) * MaxPoints), (uint)(sizeof(vec3) * Points.Length), i);
        }

        public override unsafe void Draw(Shader unused)
        {
            if (Points.Length <= 0) return;

            shader.Use();

            gl.Disable(EnableCap.CullFace);
            gl.LineWidth(LineWidth);
            gl.PointSize(LineWidth * 4);

            gl.BindVertexArray(VAO);
            if(Points.Length == 1) gl.DrawArrays(PrimitiveType.Points, 0, 1);
            else gl.DrawArrays(PrimitiveType.LineLoop, 0, (uint)Points.Length);

            gl.Enable(EnableCap.CullFace);
        }
    }
}
