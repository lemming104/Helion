namespace Helion.Client.Music;

using Helion.Audio;
using Helion.Util;
using NFluidsynth;
using NLog;
using System;
using System.IO;
using ZMusicWrapper;
using static Helion.Util.Assertion.Assert;

public class FluidSynthMusicPlayer : IMusicPlayer
{
    private const int Channels = 2;
    private const int SampleRate = 44100;
    private const int BlockLength = Channels * SampleRate;

    private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

    private bool m_disposed;
    private float m_volume = 1;
    private string m_lastFile = string.Empty;
    private string m_soundFontLoaded = string.Empty;
    private uint m_soundFontCounter = 0;

    private readonly IOutputStreamFactory m_streamFactory;
    private readonly Settings m_settings;
    private readonly Synth m_synth;

    private IOutputStream? m_stream;
    private Player? m_player;
    private bool m_enabled = true;

    public FluidSynthMusicPlayer(string soundFontFile, IOutputStreamFactory streamFactory, float sourceVolume, bool enableChorus, bool enableReverb)
    {
        m_volume = sourceVolume;

        m_settings = new Settings();
        m_synth = new(m_settings);
        m_synth.SetChorusOn(enableChorus);
        m_synth.SetReverbOn(enableReverb);

        m_streamFactory = streamFactory;

        EnsureSoundFont(soundFontFile);
    }

    public void SetVolume(float newVolume)
    {
        if (m_disposed)
            return;

        m_volume = newVolume;
        m_stream?.SetVolume(newVolume);
    }

    public bool Enabled
    {
        get => m_enabled;
        set
        {
            m_enabled = value;
            if (!value)
            {
                Stop();
            }
        }
    }

    public void SetChorusAndReverb(bool enableChorus, bool enableReverb)
    {
        m_synth.SetChorusOn(enableChorus);
        m_synth.SetReverbOn(enableReverb);
    }

    public unsafe bool Play(byte[] data, MusicPlayerOptions options)
    {
        if (m_disposed || !m_enabled)
            return false;

        try
        {
            Stop();

            if (!string.IsNullOrEmpty(m_lastFile))
                TempFileManager.DeleteFile(m_lastFile);

            m_lastFile = TempFileManager.GetFile();
            File.WriteAllBytes(m_lastFile, data);

            m_player = new(m_synth);

            if (options.HasFlag(MusicPlayerOptions.Loop))
                m_player.SetLoop(-1);

            m_player.Add(m_lastFile);
            m_player.Play();

            m_stream ??= m_streamFactory.GetOutputStream(SampleRate, Channels);
            m_stream.SetVolume(m_volume);

            m_stream.Play(FillBlockShort);

            return true;
        }
        catch (Exception ex)
        {
            Log.Warn("Error starting FluidSynth music playback.");
            Log.Info(ex);
        }

        return false;
    }

    private unsafe bool FillBlockShort(short[] sampleBlock)
    {
        if (m_player?.Status == FluidPlayerStatus.Playing)
        {
            fixed (short* ptr = sampleBlock)
                m_synth.WriteSample16(m_stream!.BlockLength, (IntPtr)ptr, 0, sampleBlock.Length, 2, (IntPtr)ptr, 1, sampleBlock.Length, 2);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Stop()
    {
        if (m_disposed)
            return;

        m_stream?.Stop();

        m_player?.Stop();
        m_player?.Join();
        m_player?.Dispose();
        m_player = null;

        m_synth.SystemReset();
    }

    public void EnsureSoundFont(string soundFontPath)
    {
        if (m_disposed)
            return;

        try
        {
            bool shouldRestart = m_player?.Status == FluidPlayerStatus.Playing;

            // Pause
            if (shouldRestart)
                OutputChanging();

            if (soundFontPath != m_soundFontLoaded)
            {
                if (!string.IsNullOrEmpty(m_soundFontLoaded))
                {
                    m_synth.UnloadSoundFont(m_soundFontCounter, true);
                    m_soundFontLoaded = string.Empty;
                }

                m_synth.LoadSoundFont(soundFontPath, true);
                for (int i = 0; i < 16; i++)
                    m_synth.SoundFontSelect(i, 0);

                m_soundFontCounter++;
                m_soundFontLoaded = soundFontPath;
            }

            // Resume
            if (shouldRestart)
                OutputChanged();
        }
        catch (Exception ex)
        {
            Log.Warn($"Could not load SoundFont {soundFontPath}.");
            Log.Info(ex.ToString());
        }
    }

    public void OutputChanging()
    {
        if (m_disposed)
            return;

        m_stream?.Stop();
        m_stream?.Dispose();
        m_stream = null;
    }

    public void OutputChanged()
    {
        if (m_disposed)
            return;

        m_stream = m_streamFactory.GetOutputStream(SampleRate, Channels);
        m_stream.SetVolume(m_volume);

        if (m_player?.Status == FluidPlayerStatus.Playing)
        {
            m_stream.Play(FillBlockShort);
        }
    }

    ~FluidSynthMusicPlayer()
    {
        FailedToDispose(this);
        PerformDispose();
    }

    public void Dispose()
    {
        PerformDispose();
        GC.SuppressFinalize(this);
    }

    protected void PerformDispose()
    {
        if (m_disposed)
            return;

        Stop(); // disposes m_player
        try
        {
            m_stream?.Dispose();
            m_synth.Dispose();
            m_settings.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warn("Error unloading FluidSynth music player");
            Log.Info(ex.ToString());
        }
        m_disposed = true;
    }
}
