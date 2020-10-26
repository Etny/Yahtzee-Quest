using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Yahtzee.Render.Textures;

namespace Yahtzee.Render
{
    class FrameBuffer : GLObject
    {
        public Texture BoundTexture { get; protected set; }
        private bool createdTexture = false;

        public RenderBuffer BoundRenderBuffer { get; protected set; }
        private bool createdRenderBuffer = false;

        public static readonly FrameBuffer Default = new FrameBuffer { ID = 0 };

        public FrameBuffer() : base() { ID = gl.GenFramebuffer(); }
        public FrameBuffer(int width, int height) : this() { CreateTexture((uint)width, (uint)height); }
        public override void Use() => gl.BindFramebuffer(FramebufferTarget.Framebuffer, ID);
        public override void Dispose()
        {
            if (createdTexture) BoundTexture.Dispose();
            if (createdRenderBuffer) BoundRenderBuffer.Dispose();

            gl.DeleteFramebuffer(ID);
        }

        public void CreateRenderBuffer(uint width, uint height, RenderBufferConfiguration config)
        {
            Use();
            BindRenderBuffer(new RenderBuffer(width, height, config.Format), config);
            createdRenderBuffer = true;
        }

        public void CreateRenderBuffer(uint width, uint height) => CreateRenderBuffer(width, height, RenderBufferConfiguration.Full);

        public void CreateTexture(uint width, uint height)
        {
            Use();
            BindTexture(new Texture(width, height));
            createdTexture = true;
        }

        public void BindTexture(Texture texture, GLEnum attachment = GLEnum.ColorAttachment0, GLEnum target = GLEnum.Texture2D, bool dispose = true)
        {
            Use();

            if (dispose && createdTexture && BoundTexture != null)
            {
                createdTexture = false;
                BoundTexture.Dispose();
            }

            BoundTexture = texture;
            gl.FramebufferTexture2D(GLEnum.Framebuffer, attachment, target, texture != null ? texture.ID : 0, 0);
        }

        public void BindRenderBuffer(RenderBuffer buffer) => BindRenderBuffer(buffer, RenderBufferConfiguration.Full);

        public void BindRenderBuffer(RenderBuffer buffer, RenderBufferConfiguration config, bool dispose = true)
        {
            Use();

            if(dispose && createdRenderBuffer && BoundRenderBuffer != null)
            {
                createdRenderBuffer = false;
                BoundRenderBuffer.Dispose();
            }

            BoundRenderBuffer = buffer;
            gl.FramebufferRenderbuffer(GLEnum.Framebuffer, config.Attachment, GLEnum.Renderbuffer, buffer.ID);
        }


        public static void UseDefault() => GL.GetApi().BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    class RenderBuffer : GLObject
    {

        public RenderBuffer() : base() { ID = gl.GenRenderbuffer(); Use(); }
        public RenderBuffer(uint width, uint height) : this() { InitializeBuffer(width, height); }
        public RenderBuffer(uint width, uint height, InternalFormat format) : this() { InitializeBuffer(width, height, format); }

        public void InitializeBuffer(uint width, uint height) => InitializeBuffer(width, height, InternalFormat.Depth24Stencil8);

        public void InitializeBuffer(uint width, uint height, InternalFormat format)
        {
            Use();
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, format, width, height);
        }

        public override void Dispose() => gl.DeleteRenderbuffer(ID);

        public override void Use() => gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, ID);
    }

    class RenderBufferConfiguration
    {

        public static RenderBufferConfiguration Full = new RenderBufferConfiguration(InternalFormat.Depth24Stencil8, GLEnum.DepthStencilAttachment);
        public static RenderBufferConfiguration Depth = new RenderBufferConfiguration(InternalFormat.DepthComponent32f, GLEnum.DepthAttachment);


        public InternalFormat Format { get; private set; }
        public GLEnum Attachment { get; private set; }


        private RenderBufferConfiguration(InternalFormat format, GLEnum attachment)
        {
            this.Format = format;
            this.Attachment = attachment;
        }
    }
}
