﻿using Helion.Geometry;
using Helion.Render.OpenGL.Renderers.Hud;
using Helion.Render.OpenGL.Renderers.World;

namespace Helion.Render.OpenGL.Surfaces
{
    /// <summary>
    /// A surface that is represented by the default framebuffer.
    /// </summary>
    public class GLDefaultRenderableSurface : GLRenderableSurface
    {
        public override Dimension Dimension => Renderer.Window.Dimension;

        public GLDefaultRenderableSurface(GLRenderer renderer, GLHudRenderer hud, GLWorldRenderer world) : 
            base(renderer, hud, world)
        {
        }

        protected override void Bind()
        {
            GLUtil.BindFramebuffer(0);
        }

        protected override void Unbind()
        {
            GLUtil.BindFramebuffer(0);
        }
    }
}
