using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using GlmSharp;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render.Models
{
    abstract class Mesh<T> : IDisposable where T : unmanaged
    {
        public T[] Vertices;
        public uint[] Indices;

        protected GL gl;

        protected uint VAO = 0, VBO = 0, EBO = 0;

        protected Mesh() { gl = GL.GetApi(); }

        public Mesh(T[] vertices, uint[] indices) : this()
        {
            Vertices = vertices;
            Indices = indices;

            SetupMesh();
        }

        public Mesh(T[] vertices) : this(vertices, null) { }

        protected virtual unsafe void SetupMesh()
        {
            VBO = gl.GenBuffer();
            if (Indices != null) EBO = gl.GenBuffer();
            VAO = gl.GenVertexArray();

            gl.BindVertexArray(VAO);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            fixed (void* i = &Vertices[0])
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(Vertices.Length * sizeof(T)), i, BufferUsageARB.StaticDraw);

            if (Indices != null)
            {
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EBO);
                fixed (void* i = &Indices[0])
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }

            SetupVertexAttributePointers();
        }

        protected abstract void SetupVertexAttributePointers();

        public virtual void Dispose()
        {
            if (VBO != 0) gl.DeleteBuffer(VBO);
            if (Indices != null) gl.DeleteBuffer(EBO);
            if (VAO != 0) gl.DeleteVertexArray(VAO);
        }

        public abstract void Draw(Shader shader = null, int count = 1);
    }

}
