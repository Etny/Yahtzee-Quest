using GlmSharp;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Yahtzee.Game;
using Yahtzee.Main;
using Yahtzee.Render;

namespace Yahtzee.Game.Physics
{
    class CollisionMesh : Mesh
    {
        public vec3[] CollisionVertices;
        public vec3[] DrawVertices;

        public int highlight = -1;

        private Shader shader;

        public bool Overlapping = false;

        public CollisionMesh(vec3[] vertices, uint[] indices) : base()
        {
            this.DrawVertices = vertices;
            this.Indices = indices;

            this.shader = ShaderRepository.GetShader("Debug/Line/line");

            List<vec3> temp = new List<vec3>();

            foreach(vec3 v in vertices)
                if (!temp.Contains(v)) temp.Add(v);

            CollisionVertices = temp.ToArray();

            setupCollisionMesh();
        }

        [Conditional("DEBUG")]
        protected unsafe void setupCollisionMesh()
        {
            VBO = gl.GenBuffer();
            if (Indices != null) EBO = gl.GenBuffer();
            VAO = gl.GenVertexArray();

            gl.BindVertexArray(VAO);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            fixed (void* i = &DrawVertices[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(DrawVertices.Length * sizeof(vec3)), i, BufferUsageARB.StaticDraw);

            if (Indices != null)
            {
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
                fixed (void* i = &Indices[0])
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }

            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(vec3), (void*)0);
            gl.EnableVertexAttribArray(0);
        }


        [Conditional("DEBUG")]
        public unsafe void DrawOutline(Transform t)
        {
            shader.Use();
            shader.SetMat4("model", t.ModelMatrix);
            shader.SetBool("overlapping", Overlapping);

            gl.Disable(EnableCap.CullFace);
            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line);
            gl.LineWidth(5);

            gl.BindVertexArray(VAO);
            if (Indices != null) gl.DrawElements(PrimitiveType.Triangles, (uint)DrawVertices.Length, DrawElementsType.UnsignedInt, (void*)0);
            else gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)DrawVertices.Length);

            if(highlight >= 0)
            {
                gl.PointSize(40);
                shader.SetBool("overlapping", true);
                gl.DrawArrays(PrimitiveType.Points, highlight, 1);
            }

            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
            gl.Enable(EnableCap.CullFace);

        }


    }
}
