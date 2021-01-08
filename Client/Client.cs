﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Helion.Audio;
using Helion.Audio.Impl;
using Helion.Client.Music;
using Helion.Layer;
using Helion.Resources.Archives.Collection;
using Helion.Resources.Archives.Locator;
using Helion.Util;
using Helion.Util.Configs;
using Helion.Util.Consoles;
using NLog;
using OpenTK.Windowing.Common;
using static Helion.Util.Assertion.Assert;

namespace Helion.Client
{
    public partial class Client : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ArchiveCollection m_archiveCollection;
        private readonly IAudioSystem m_audioSystem;
        private readonly CommandLineArgs m_commandLineArgs;
        private readonly Config m_config;
        private readonly HelionConsole m_console;
        private readonly GameLayerManager m_layerManager;
        private readonly Window m_window;
        private bool m_disposed;

        private Client(CommandLineArgs commandLineArgs, Config config, HelionConsole console, IAudioSystem audioSystem,
            IMusicPlayer musicPlayer)
        {
            m_commandLineArgs = commandLineArgs;
            m_config = config;
            m_console = console;
            m_audioSystem = audioSystem;
            m_archiveCollection = new ArchiveCollection(new FilesystemArchiveLocator(config));
            m_layerManager = new GameLayerManager(config, m_archiveCollection, m_console);
            m_window = new Window(config);

            m_console.OnConsoleCommandEvent += Console_OnCommand;
            m_window.RenderFrame += Window_MainLoop;
        }

        ~Client()
        {
            FailedToDispose(this);
            PerformDispose();
        }

        private void CheckForErrors()
        {
            // TODO: ALAudioSystem.CheckForErrors();
        }

        private void HandleInput()
        {
            // TODO
        }

        private void RunLogic()
        {
            // TODO
        }

        private bool ShouldRender()
        {
            return m_fpsLimitValue <= 0 || m_fpsLimit.ElapsedTicks * StopwatchFrequencyValue / Stopwatch.Frequency >= m_fpsLimitValue;
        }

        private void Render()
        {
            if (!ShouldRender())
                return;

            m_fpsLimit.Restart();
            // TODO: Issue render commands.
            m_window.SwapBuffers();
            m_fpsTracker.FinishFrame();
        }

        private void Window_MainLoop(FrameEventArgs frameEventArgs)
        {
            CheckForErrors();

            HandleInput();
            RunLogic();
            Render();
        }

        /// <summary>
        /// Runs the client until the client requests the game exit.
        /// </summary>
        public void Run()
        {
            Initialize();
            m_window.Run();
        }

        private void PerformDispose()
        {
            if (m_disposed)
                return;

            m_console.OnConsoleCommandEvent -= Console_OnCommand;
            m_window.RenderFrame -= Window_MainLoop;

            m_layerManager.Dispose();
            m_window.Dispose();

            m_disposed = true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            PerformDispose();
        }

        [Conditional("DEBUG")]
        private static void ForceFinalizersIfDebugMode()
        {
            // Apparently garbage collection only happens if we call it twice,
            // since they are not truly garbage collected until the second pass
            // over the objects.
            //
            // We also do this because we want to have assertion failures occur
            // if we accidentally forget to dispose of anything. At termination
            // of the program, the finalizers might not be called and we'd not
            // know if we failed to Dispose() something. At least in the debug
            // mode we will get assertions that trigger if we force all of the
            // finalizers to run.
            //
            // This should mean that in debug mode, the following invocations
            // of the GC will cause us to be alerted if we ever fail to dispose
            // of anything.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void LogClientInfo()
        {
            Log.Info("{0} v{1}", Constants.ApplicationName, Constants.ApplicationVersion);

            Log.Info("Processor: {0} {1}", Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"), RuntimeInformation.OSArchitecture);
            Log.Info("Processor count: {0}", Environment.ProcessorCount);
            Log.Info("OS: {0} {1} (running {2})", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "x64" : "x86", Environment.Is64BitProcess ? "x64" : "x86");

            if (Environment.Is64BitOperatingSystem != Environment.Is64BitProcess)
            {
                Log.Warn("Using a different bit architecture for the process than the OS supports!");
                Log.Warn("This may lead to performance issues.");
            }
        }

        private static void LogAnyCommandLineErrors(CommandLineArgs commandLineArgs)
        {
            if (!commandLineArgs.ErrorWhileParsing)
                return;

            Log.Error("Bad command line arguments detected:");
            commandLineArgs.Errors.ForEach(Log.Error);
        }

        public static void Main(string[] args)
        {
            CommandLineArgs commandLineArgs = CommandLineArgs.Parse(args);
            Logging.Initialize(commandLineArgs);
            LogClientInfo();
            LogAnyCommandLineErrors(commandLineArgs);

            try
            {
                using Config config = new();
                ArchiveCollection archiveCollection = new(new FilesystemArchiveLocator(config));
                using HelionConsole console = new(config);
                using IMusicPlayer musicPlayer = new MidiMusicPlayer(config);
                using IAudioSystem audioPlayer = new OpenALAudioSystem(config, archiveCollection, musicPlayer);
                using Client client = new(commandLineArgs, config, console, audioPlayer, musicPlayer);
                client.Run();
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error: {0}", e.Message);
#if DEBUG
                throw;
#else
                // TODO: Maybe make a Win32 popup saying "Oops" so it doesn't just vanish
#endif
            }
            finally
            {
                ForceFinalizersIfDebugMode();
                LogManager.Shutdown();
            }
        }
    }
}
