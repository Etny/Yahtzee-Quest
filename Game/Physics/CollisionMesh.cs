using GlmSharp;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Yahtzee.Game;
using Yahtzee.Main;
using Yahtzee.Render;
using Yahtzee.Render.Models;

namespace Yahtzee.Game.Physics
{
    class CollisionMesh : Mesh<Vertex>
    {
        public vec3[] CollisionVertices;
        public vec3[] DrawVertices;

        public int highlight = -1;

        private Shader shader;

        public bool Overlapping = false;
        public vec3 NormalColor = new vec3(1, 0.9f, 0);
        public vec3 OverlappingColor = new vec3(1, .3f, 0);

        public CollisionMesh(vec3[] vertices, uint[] indices) : base()
        {
            this.DrawVertices = vertices;
            this.Indices = indices;

            this.shader = ShaderRepository.GetShader("Debug/Line/line");

            List<vec3> temp = new List<vec3>();

            foreach(vec3 v in vertices)
                if (!temp.Contains(v)) temp.Add(v);

            CollisionVertices = temp.ToArray();

            SetupMesh();
        }

        protected unsafe override void SetupMesh()
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
            shader.SetVec4("color", Overlapping ? new vec4(OverlappingColor, 1) : new vec4(NormalColor, 1));

            gl.Disable(EnableCap.CullFace);
            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line);
            gl.LineWidth(5);

            gl.BindVertexArray(VAO);
            if (Indices != null) gl.DrawElements(PrimitiveType.Triangles, (uint)DrawVertices.Length, DrawElementsType.UnsignedInt, (void*)0);
            else gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)DrawVertices.Length);

            //if(highlight >= 0)
            //{
            //    gl.PointSize(40);
            //    shader.SetBool("overlapping", true);
            //    gl.DrawArrays(PrimitiveType.Points, highlight, 1);
            //}

            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
            gl.Enable(EnableCap.CullFace);

        }

        protected override void SetupVertexAttributePointers()
        {
            throw new System.NotImplementedException();
        }

        public override void Draw(Shader shader = null, int count = 1)
        {
            throw new System.NotImplementedException();
        }
    }
}
