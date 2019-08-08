using System;
using System.Diagnostics;
using System.Drawing;
using Helion.Render.Commands;
using Helion.Render.Commands.Types;
using Helion.Render.OpenGL.Context;
using Helion.Render.OpenGL.Context.Types;
using Helion.Render.OpenGL.Util;
using Helion.Resources.Archives.Collection;
using Helion.Util.Configuration;
using Helion.Util.Geometry;
using NLog;
using static Helion.Util.Assertion.Assert;

namespace Helion.Render.OpenGL
{
    public class GLRenderer : IRenderer, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static bool InfoPrinted;
        
        private readonly Config m_config;
        private readonly ArchiveCollection m_archiveCollection;
        private readonly GLCapabilities m_capabilities;
        private readonly GLFunctions gl;
        private bool m_disposed;
        
        public GLRenderer(Config config, ArchiveCollection archiveCollection, GLFunctions functions)
        {
            m_config = config;
            m_archiveCollection = archiveCollection;
            m_capabilities = new GLCapabilities(functions);
            gl = functions;

            PrintGLInfo();
            SetGLStates();
            SetGLDebugger();
        }

        ~GLRenderer()
        {
            Fail("Did not dispose of GLRenderer, finalizer run when it should not be");
            ReleaseUnmanagedResources();
        }

        public void Render(RenderCommands renderCommands)
        {
            foreach (IRenderCommand renderCommand in renderCommands.GetCommands())
            {
                switch (renderCommand)
                {
                case ClearRenderCommand cmd:
                    HandleClearCommand(cmd);
                    break;
                case DrawWorldCommand cmd:
                    // TODO
                    break;
                case ViewportCommand cmd:
                    HandleViewportCommand(cmd);
                    break;
                default:
                    Fail($"Unsupported render command type: {renderCommand}");
                    break;
                }
            }
            
            GLHelper.AssertNoGLError(gl);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
        
        private void PrintGLInfo()
        {
            if (InfoPrinted)
                return;
            
            Log.Info("Loaded OpenGL v{0}", m_capabilities.Version);
            Log.Info("OpenGL Shading Language: {0}", m_capabilities.Info.ShadingVersion);
            Log.Info("Vendor: {0}", m_capabilities.Info.Vendor);
            Log.Info("Hardware: {0}", m_capabilities.Info.Renderer);

            InfoPrinted = true;
        }
        
        private void SetGLStates()
        {
            gl.Enable(EnableType.DepthTest);
            
            if (m_config.Engine.Render.Multisample.Enable)
                gl.Enable(EnableType.Multisample);

            if (m_capabilities.Version.Supports(3, 2))
                gl.Enable(EnableType.TextureCubeMapSeamless);

            gl.Enable(EnableType.Blend);
            gl.BlendFunc(BlendingFactorType.SrcAlpha, BlendingFactorType.OneMinusSrcAlpha);

            gl.Enable(EnableType.CullFace);
            gl.FrontFace(FrontFaceType.CounterClockwise);
            gl.CullFace(CullFaceType.Back);
            gl.PolygonMode(PolygonFaceType.FrontAndBack, PolygonModeType.Fill);
        }
        
        [Conditional("DEBUG")]
        private void SetGLDebugger()
        {
            // Note: This means it's not set if `RenderDebug` changes. As far
            // as I can tell, we can't unhook actions, but maybe we could do
            // some glDebugControl... setting that changes them all to don't
            // cares if we have already registered a function? See:
            // https://www.khronos.org/opengl/wiki/GLAPI/glDebugMessageControl
            if (!m_capabilities.Version.Supports(4, 3) || !m_config.Engine.Developer.RenderDebug) 
                return;
            
            gl.Enable(EnableType.DebugOutput);
            gl.Enable(EnableType.DebugOutputSynchronous);
            
            // TODO: We should filter messages we want to get since this could
            //       pollute us with lots of messages and we wouldn't know it.
            //       https://www.khronos.org/opengl/wiki/GLAPI/glDebugMessageControl
            gl.DebugMessageCallback((severity, message) =>
            {
                switch (severity)
                {
                case DebugLevel.High:
                case DebugLevel.Medium:
                    Log.Error("[GLDebug type={0}] {1}", severity, message);
                    break;
                case DebugLevel.Low:
                    Log.Warn("[GLDebug type={0}] {1}", severity, message);
                    break;
                }
            });
        }
        
        private void HandleClearCommand(ClearRenderCommand clearRenderCommand)
        {
            Color color = clearRenderCommand.ClearColor;
            gl.ClearColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            
            ClearType clearMask = 0;
            if (clearRenderCommand.Color)
                clearMask |= ClearType.ColorBufferBit;
            if (clearRenderCommand.Depth)
                clearMask |= ClearType.DepthBufferBit;
            if (clearRenderCommand.Stencil)
                clearMask |= ClearType.StencilBufferBit;
            
            gl.Clear(clearMask);
        }
        
        private void HandleViewportCommand(ViewportCommand viewportCommand)
        {
            Vec2I offset = viewportCommand.Offset;
            Dimension dimension = viewportCommand.Dimension;
            gl.Viewport(offset.X, offset.Y, dimension.Width, dimension.Height);
        }

        private void ReleaseUnmanagedResources()
        {
            Precondition(!m_disposed, "Trying to dispose the GLRenderer twice");
            
            // TODO: Release unmanaged resources here.

            m_disposed = true;
        }
    }
}