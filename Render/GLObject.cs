using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render
{
    abstract class GLObject : IDisposable
    {
        public uint ID { get; protected set; }

        protected GL gl;

        public GLObject() { gl = GL.GetApi(); }

        public abstract void Use();

        public abstract void Dispose();
    }
}
