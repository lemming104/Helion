using System;
using System.Collections.Generic;
using Helion.Client.Input;
using Helion.Geometry;
using Helion.Geometry.Vectors;
using Helion.Render;
using Helion.Render.Legacy;
using Helion.Render.Legacy.Context;
using Helion.Render.OpenGL;
using Helion.Resources.Archives.Collection;
using Helion.Util;
using Helion.Util.Configs;
using Helion.Util.Timing;
using Helion.Window;
using Helion.Window.Input;
using NLog;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using static Helion.Util.Assertion.Assert;

namespace Helion.Client;

/// <summary>
/// A window that emits events and handles rendering.
/// </summary>
/// <remarks>
/// Allows us to override and extend the underlying game window as needed.
/// </remarks>
public class Window : GameWindow, IWindow
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public IInputManager InputManager => m_inputManager;
    public IRenderer Renderer { get; }
    public Dimension Dimension => new(Bounds.Max.X - Bounds.Min.X, Bounds.Max.Y - Bounds.Min.Y);
    public Dimension FramebufferDimension => Dimension; // Note: In the future, use `GLFW.GetFramebufferSize` maybe.
    private readonly IConfig m_config;
    private readonly bool IgnoreMouseEvents;
    private readonly InputManager m_inputManager = new();
    private bool m_disposed;
    private Vec2F m_prevScroll = Vec2F.Zero;

    public Window(IConfig config, ArchiveCollection archiveCollection, FpsTracker tracker) :
        base(MakeGameWindowSettings(), MakeNativeWindowSettings(config))
    {
        Log.Debug("Creating client window");

        m_config = config;
        CursorVisible = !config.Mouse.Focus;
        Renderer = CreateRenderer(config, archiveCollection, tracker);
        IgnoreMouseEvents = config.Mouse.RawInput;
        CursorGrabbed = config.Mouse.Focus;
        VSync = config.Render.VSync ? VSyncMode.Adaptive : VSyncMode.Off;
        RenderFrequency = config.Render.MaxFPS;

        KeyDown += Window_KeyDown;
        KeyUp += Window_KeyUp;
        MouseDown += Window_MouseDown;
        MouseMove += Window_MouseMove;
        MouseUp += Window_MouseUp;
        MouseWheel += Window_MouseWheel;
        TextInput += Window_TextInput;

        m_config.Render.MaxFPS.OnChanged += OnMaxFpsChanged;
        m_config.Render.VSync.OnChanged += OnVSyncChanged;
    }

    public List<MonitorData> GetMonitors(out MonitorData? currentMonitor)
    {
        currentMonitor = null;
        var currentHandle = Monitors.GetMonitorFromWindow(this);
        List<MonitorData> monitors = new(Monitors.Count);
        for(int i = 0; i < Monitors.Count; i++)
        {
            if (!Monitors.TryGetMonitorInfo(i, out MonitorInfo info))
                continue;

            var monitorData = new MonitorData(i, info.HorizontalResolution, info.VerticalResolution, info.Handle);
            monitors.Add(monitorData);

            if (info.Handle.Pointer == currentHandle.Pointer)
                currentMonitor = monitorData;
        }

        return monitors;
    }

    ~Window()
    {
        FailedToDispose(this);
        PerformDispose();
    }

    private static GameWindowSettings MakeGameWindowSettings()
    {
        return new GameWindowSettings
        {
            RenderFrequency = 500
        };
    }

    private static NativeWindowSettings MakeNativeWindowSettings(IConfig config)
    {
        (int windowWidth, int windowHeight) = config.Window.Dimension.Value;

        var settings = new NativeWindowSettings
        {
            Profile = Constants.UseNewRenderer ? ContextProfile.Any : ContextProfile.Core,
            APIVersion = Constants.UseNewRenderer ? new Version(2, 0) : new Version(3, 3),
            Flags = config.Developer.Render.Debug ? ContextFlags.Debug : ContextFlags.Default,
            IsFullscreen = config.Window.State == WindowState.Fullscreen,
            NumberOfSamples = config.Render.Multisample.Value,
            Size = new Vector2i(windowWidth, windowHeight),
            Title = Constants.ApplicationName,
            WindowBorder = config.Window.Border,
            WindowState = config.Window.State,
        };

        SetDisplay(config, settings);
        return settings;
    }

    private static void SetDisplay(IConfig config, NativeWindowSettings settings)
    {
        if (config.Window.Display.Value <= 0)
            return;

        if (!Monitors.TryGetMonitorInfo(config.Window.Display.Value - 1, out MonitorInfo monitorInfo))
        {
            Log.Error($"Invalid display number: {config.Window.Display.Value}");
            return;
        }
        
        settings.CurrentMonitor = monitorInfo.Handle;
    }

    public void SetGrabCursor(bool set) => CursorGrabbed = set;

    private IRenderer CreateRenderer(IConfig config, ArchiveCollection archiveCollection, FpsTracker tracker)
    {
        if (Constants.UseNewRenderer)
            return new GLRenderer(config, this, archiveCollection);
        return new GLLegacyRenderer(this, config, archiveCollection, new OpenTKGLFunctions(), tracker);
    }

    private void Window_KeyUp(KeyboardKeyEventArgs args)
    {
        Key key = OpenTKInputAdapter.ToKey(args.Key);
        if (key != Key.Unknown)
            m_inputManager.SetKeyUp(key);
    }

    private void Window_KeyDown(KeyboardKeyEventArgs args)
    {
        Key key = OpenTKInputAdapter.ToKey(args.Key);
        if (key != Key.Unknown)
            m_inputManager.SetKeyDown(key);
    }

    private void Window_MouseDown(MouseButtonEventArgs args)
    {
        Key key = OpenTKInputAdapter.ToMouseKey(args.Button);
        if (key != Key.Unknown)
            m_inputManager.SetKeyDown(key);
    }

    private void Window_MouseMove(MouseMoveEventArgs args)
    {
        if (IgnoreMouseEvents)
            return;

        if (m_config.Mouse.Focus)
        {
            int centerX = Size.X / 2;
            int centerY = Size.Y / 2;
            Vec2F movement = (centerX - MouseState.X, centerY - MouseState.Y);
            m_inputManager.AddMouseMovement(movement.Int);
            MousePosition = new Vector2(centerX, centerY);
        }
        else
        {
            Vec2F movement = (-args.Delta.X, -args.Delta.Y);
            m_inputManager.AddMouseMovement(movement.Int);
        }
    }

    private void Window_MouseUp(MouseButtonEventArgs args)
    {
        Key key = OpenTKInputAdapter.ToMouseKey(args.Button);
        if (key != Key.Unknown)
            m_inputManager.SetKeyUp(key);
    }

    private void Window_MouseWheel(MouseWheelEventArgs args)
    {
        m_inputManager.AddMouseScroll(args.OffsetY - m_prevScroll.Y);
        m_prevScroll.X = args.OffsetX;
        m_prevScroll.Y = args.OffsetY;
    }

    private void Window_TextInput(TextInputEventArgs args)
    {
        m_inputManager.AddTypedCharacters(args.AsString);
    }

    public void HandleRawMouseMovement(int x, int y)
    {
        m_inputManager.AddMouseMovement((x, y));
    }

    private void OnMaxFpsChanged(object? sender, int maxFps)
    {
        RenderFrequency = maxFps;
    }

    private void OnVSyncChanged(object? sender, bool useVSync)
    {
        VSync = useVSync ? VSyncMode.Adaptive : VSyncMode.Off;
    }

    private void PerformDispose()
    {
        if (m_disposed)
            return;

        KeyDown -= Window_KeyDown;
        KeyUp -= Window_KeyUp;
        MouseDown -= Window_MouseDown;
        MouseMove -= Window_MouseMove;
        MouseUp -= Window_MouseUp;
        MouseWheel -= Window_MouseWheel;
        TextInput -= Window_TextInput;

        m_config.Render.MaxFPS.OnChanged -= OnMaxFpsChanged;
        m_config.Render.VSync.OnChanged -= OnVSyncChanged;

        m_disposed = true;
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
        PerformDispose();
    }
}
