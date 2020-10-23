using GlmSharp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Models;

namespace Yahtzee.Render.UI
{
    class QuadMesh : Mesh<QuadVertex>
    {

        private static readonly vec2[] BaseVerts = { new vec2(-1, -1), new vec2(-1, 1), new vec2(1, 1), new vec2(1, -1) };
        private static readonly vec2[] BaseTexCoords = { new vec2(0, 0), new vec2(0, 1), new vec2(1, 1), new vec2(1, 0) };
        private static readonly uint[] BaseIndices = { 1, 0, 3, 1, 3, 2};

        public vec2 Size { get; protected set; }

        public QuadMesh(float width, float height) : base()
        {
            Size = new vec2(width, height);

            QuadVertex[] verts = new QuadVertex[4];
            for (int i = 0; i < 4; i++) 
                verts[i] = new QuadVertex() { Position = BaseVerts[i] * Size, TexCoords = BaseTexCoords[i] };

            Vertices = verts;
            Indices = BaseIndices;

            SetupMesh();
        }

        protected unsafe override void SetupVertexAttributePointers()
        {
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(QuadVertex), (void*)0);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(QuadVertex), (void*)sizeof(vec2));
            gl.EnableVertexAttribArray(0);
            gl.EnableVertexAttribArray(1);
        }

        public unsafe override void Draw(Shader shader, int count = 1)
        {
            shader.Use();
            gl.BindVertexArray(VAO);
            gl.DrawElements((GLEnum)PrimitiveType.Triangles, (uint)Indices.Length, (GLEnum)DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    struct QuadVertex
    {
        public vec2 Position;
        public vec2 TexCoords;
    }
}
