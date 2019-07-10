using System;
using Helion.Render.OpenGL.Util;
using OpenTK.Graphics.OpenGL;
using Buffer = System.Buffer;

namespace Helion.Render.OpenGL.Buffers
{
    // TODO: Perform sanity checks against GL_MAX_TEXTURE_BUFFER_SIZE.
    
    public class TextureBufferObject<T> : BufferObject<T> where T : struct
    {
        private readonly int tbo;
        
        public TextureBufferObject(GLCapabilities capabilities, BufferUsageHint usageHint = BufferUsageHint.DynamicDraw) : 
            base(capabilities, BufferTarget.TextureBuffer, usageHint, GL.GenBuffer())
        {
            tbo = GL.GenTexture();
        }

        protected override void ReleaseUnmanagedResources()
        {
            GL.DeleteTexture(tbo);
            
            base.ReleaseUnmanagedResources();
        }

        // TODO: Want to support updating a range and pushing that through to
        //       the texture buffer. Can we do that with this[] indexing and
        //       not kill performance? Use glMapBuffer of glSubBufferData...?
        
        // TODO: Should we also use PBOs? Can we even? Is that safe to update
        //       the buffer or do we need a fence?

        private void BindTexture(TextureUnit textureUnit)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.TextureBuffer, tbo);
            
            Bind();
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, BufferHandle);
        }

        private void UnbindTexture()
        {
            GL.BindTexture(TextureTarget.TextureBuffer, 0);
            Unbind();
        }

        public void BindTextureAnd(TextureUnit textureUnit, Action func)
        {
            BindTexture(textureUnit);
            func.Invoke();
            UnbindTexture();
        }
    }
}