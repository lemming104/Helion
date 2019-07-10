using System;
using Helion.Maps.Geometry.Lines;
using Helion.Render.OpenGL.Renderers.World.Geometry.Static;
using Helion.Render.OpenGL.Texture;
using Helion.Render.OpenGL.Util;
using Helion.Render.Shared;
using Helion.Render.Shared.World;
using Helion.Util;
using Helion.Util.Configuration;
using Helion.Util.Geometry;
using Helion.World;
using OpenTK;

namespace Helion.Render.OpenGL.Renderers.World.Geometry
{
    public class WorldGeometryRenderer : IDisposable
    {
        private readonly Config m_config;
        private readonly GLTextureManager m_textureManager;
        private readonly StaticGeometryRenderer m_staticGeometryRenderer;

        public WorldGeometryRenderer(Config config, GLCapabilities capabilities, GLTextureManager textureManager)
        {
            m_config = config;
            m_textureManager = textureManager;
            m_staticGeometryRenderer = new StaticGeometryRenderer(capabilities, textureManager);
        }

        ~WorldGeometryRenderer()
        {
            ReleaseUnmanagedResources();
        }
        
        public void Render(RenderInfo renderInfo)
        {
            Matrix4 mvp = GLRenderer.CreateMVP(renderInfo, (float)m_config.Engine.Render.FieldOfView);
            
            m_staticGeometryRenderer.Render(mvp);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        internal void UpdateToWorld(WorldBase world)
        {
            world.Map.Lines.ForEach(Triangulate);
        }

        private void Triangulate(Line line)
        {
            // TODO: Temporary!
            if (line.TwoSided)
                return;
            
            m_staticGeometryRenderer.AddLine(WorldTriangulator.Triangulate(line, TextureFinder));
            
            Dimension TextureFinder(UpperString name) => m_textureManager.GetWallTexture(name).Dimension;
        }

        private void ReleaseUnmanagedResources()
        {
            m_staticGeometryRenderer.Dispose();
        }
    }
}