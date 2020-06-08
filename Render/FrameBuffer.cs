using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yahtzee.Render
{
    class FrameBuffer : GLObject
    {
        public Texture BoundTexture { get; protected set; }
        private bool createdTexture = false;

        public FrameBuffer() : base() { ID = gl.CreateFramebuffer(); }

        public override void Use() => gl.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
        public override void Dispose() => gl.DeleteFramebuffer(ID);

        public void CreateTexture(uint width, uint height)
        {
            BindTexture(new Texture(width, height), GLEnum.ColorAttachment0);
            createdTexture = true;
        }

        public void BindTexture(Texture texture, GLEnum attachment)
        {
            if (createdTexture && BoundTexture != null)
            {
                createdTexture = false;
                BoundTexture.Dispose();
            }

            BoundTexture = texture;
            gl.FramebufferTexture(GLEnum.Framebuffer, attachment, texture.ID, 0);
        }


        public static void BindDefault() => GL.GetApi().BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
