using System;
using System.Collections.Generic;
using System.Text;
using Helion.Graphics;
using Helion.Util.CommandLine;
using Helion.Util.Configs;
using Helion.Util.Extensions;
using Helion.Util.Timing;
using Helion.Util.Loggers;
using NLog;
using NLog.Config;
using NLog.Targets;
using static Helion.Util.Assertion.Assert;

namespace Helion.Util.Consoles;

/// <summary>
/// A console object that accepts input, emits console commands, and will
/// be able to register for log messages to track.
/// </summary>
/// <remarks>
/// This class is not intended to handle any rendering. Its only job is to
/// be a medium for user pressed characters and messages from a variety of
/// message emitters (ex: loggers).
/// </remarks>
public class HelionConsole : Target
{
    private const string TargetName = "HelionConsole";
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static readonly Color DebugColor = (255, 128, 255);
    private static readonly Color TraceColor = (255, 200, 255);

    /// <summary>
    /// All the messages that have been received thus far.
    /// </summary>
    /// <remarks>
    /// This will never exceed <see cref="m_capacity"/>. Any messages at the
    /// end of the list will be removed once this grows past the capacity
    /// value.
    /// </remarks>
    public readonly LinkedList<ConsoleMessage> Messages = new();

    /// <summary>
    /// A list of all the input that has been submitted. This allows us to
    /// get the commands we've sent in the past. The front of the list is
    /// the most recent command.
    /// </summary>
    public readonly List<string> SubmittedInput = new();

    /// <summary>
    /// The clock epoch in nanoseconds when this was last closed.
    /// </summary>
    /// <remarks>
    /// This is set by other viewers as a marker when it was last closed.
    /// We need this because we don't want console messages that were just
    /// viewed in a console renderer to appear in the messages area on the
    /// screen. This should be set with something like Ticker.NanoTime()
    /// when the console is closed from a viewer.
    /// </remarks>
    public long LastClosedNanos;

    /// <summary>
    /// Gets the current input.
    /// </summary>
    /// <remarks>
    /// This causes a copy allocation of the current input text.
    /// </remarks>
    public string Input => m_input.ToString();

    /// <summary>
    /// The event handler that emits console commands on user input.
    /// </summary>
    public event EventHandler<ConsoleCommandEventArgs>? OnConsoleCommandEvent;

    private readonly IConfig? m_config;
    private readonly StringBuilder m_input = new();
    private readonly DataCache m_dataCache;
    private int m_capacity;
    private bool m_disposed;

    public HelionConsole(DataCache dataCache, IConfig? cfg = null, CommandLineArgs? args = null)
    {
        Name = TargetName;
        m_config = cfg;
        m_capacity = m_config?.Console.MaxMessages ?? 128;
        m_dataCache = dataCache;

        if (m_config != null)
        {
            // I have no idea why this keeps thinking it is null when it is
            // not possible, I'll suppress it.
            m_config.Console.MaxMessages.OnChanged += OnMaxMessagesChanged;
            AddToLogger(args);
        }

        HelionLog.Message += HelionMessageLogger_Message;
    }

    ~HelionConsole()
    {
        FailedToDispose(this);
        PerformDispose();
    }

    /// <summary>
    /// Removes an input character, if any.
    /// </summary>
    public void RemoveInputCharacter()
    {
        if (m_input.Length > 0)
            m_input.Remove(m_input.Length - 1, 1);
    }

    /// <summary>
    /// Clears the input text.
    /// </summary>
    public void ClearInputText()
    {
        m_input.Clear();
    }

    /// <summary>
    /// Submits the current input text by firing an event and clears the
    /// input.
    /// </summary>
    public void SubmitInputText()
    {
        string inputText = m_input.ToString().Trim();
        ClearInputText();

        if (inputText.Empty())
            return;

        CacheSubmittedInput(inputText);
        OnConsoleCommandEvent?.Invoke(this, new ConsoleCommandEventArgs(inputText));
    }

    /// <summary>
    /// Submits a new set of input without touching the existing input. The
    /// input is not cached.
    /// </summary>
    /// <param name="command">The standalone command to execute.</param>
    public void SubmitInputText(string command)
    {
        if (command.Empty())
            return;

        OnConsoleCommandEvent?.Invoke(this, new ConsoleCommandEventArgs(command));
    }

    public void AddMessage(string message) => AddMessage(Color.White, message);

    /// <summary>
    /// Adds a new message to the console.
    /// </summary>
    /// <remarks>
    /// If this message causes the console to exceed the capacity, then it
    /// will remove the older messages to make space for this message.
    /// </remarks>
    /// <param name="color">The color of the message..</param>
    /// <param name="message">The message to add.</param>
    public void AddMessage(Color color, string message)
    {
        if (message.Length == 0)
            return;

        lock (Messages)
        {
            var node = m_dataCache.GetConsoleMessageNode(m_dataCache.GetConsoleMessage(message, Ticker.NanoTime(), color));
            Messages.AddFirst(node);
            RemoveExcessMessagesIfAny();
        }
    }

    /// <summary>
    /// Adds a single character to the input.
    /// </summary>
    /// <remarks>
    /// Invalid characters are not supported. For example, adding a null
    /// terminator will cause nothing to happen.
    /// </remarks>
    /// <param name="c">The character to add.</param>
    public void AddInput(char c)
    {
        if (IsInputSubmissionCharacter(c))
            SubmitInputText();
        else if (IsBackspaceCharacter(c))
            RemoveInputCharacter();
        else if (IsTextCharacter(c))
            m_input.Append(c);
    }

    /// <summary>
    /// Adds the provided string to the input.
    /// </summary>
    /// <remarks>
    /// See <see cref="AddInput(char)"/> for further remarks.
    /// </remarks>
    /// <param name="text">The text to add.</param>
    public void AddInput(string text)
    {
        foreach (var c in text.ToCharArray())
            AddInput(c);
    }

    protected override void Write(LogEventInfo logEvent) =>
        HelionMessageLogger_Message(null, new(logEvent.FormattedMessage, ToMessageLevel(logEvent.Level.Ordinal)));

    private static MessageLevel ToMessageLevel(int logLevel)
    {
        switch (logLevel)
        {
            case 0:
                return MessageLevel.Trace;
            case 1:
                return MessageLevel.Debug;
            case 2:
                return MessageLevel.Info;
            case 3:
                return MessageLevel.Warning;
            case >= 4 and <= 6:
                return MessageLevel.Error;
        }

        return MessageLevel.Info;
    }

    private void HelionMessageLogger_Message(object? sender, MessageLogEvent e)
    {
        switch (e.Level)
        {
            case MessageLevel.Trace:
                AddMessage(TraceColor, e.Message);
                break;
            case MessageLevel.Debug:
                AddMessage(DebugColor, e.Message);
                break;
            case MessageLevel.Info:
                AddMessage(Color.White, e.Message);
                break;
            case MessageLevel.Warning:
                AddMessage(Color.Yellow, e.Message);
                break;
            case MessageLevel.Error:
                AddMessage(Color.Red, e.Message);
                break;
        }
    }

    private static bool IsTextCharacter(char c) => c >= 32 && c < 127;

    private static bool IsBackspaceCharacter(char c) => c == 8;

    private static bool IsInputSubmissionCharacter(char c) => c == '\n' || c == '\r';

    private void OnMaxMessagesChanged(object? sender, int newMaxMessage)
    {
        m_capacity = Math.Max(1, newMaxMessage);
        RemoveExcessMessagesIfAny();
    }

    private void AddToLogger(CommandLineArgs? args)
    {
        LoggingRule rule = new("*", LogLevel.Info, this);
        if (args?.LogLevel != null)
        {
            if (args.LogLevel.EqualsIgnoreCase("trace"))
                rule = new LoggingRule("*", LogLevel.Trace, this);
            else if (args.LogLevel.EqualsIgnoreCase("trace"))
                rule = new LoggingRule("*", LogLevel.Debug, this);
        }

        LogManager.Configuration.LoggingRules.Add(rule);
        LogManager.Configuration.AddTarget(TargetName, this);
        LogManager.ReconfigExistingLoggers();
    }

    private void RemoveLogger()
    {
        LogManager.Configuration.RemoveTarget(TargetName);
    }

    private void RemoveExcessMessagesIfAny()
    {
        while (Messages.Count > m_capacity)
        {
            var node = Messages.Last;
            if (node != null)
            {
                m_dataCache.FreeConsoleMessage(node.Value);
                m_dataCache.FreeConsoleMessageNode(node);
            }
            Messages.RemoveLast();
        }
    }

    private void CacheSubmittedInput(string inputText)
    {
        RemoveExcessSubmittedInputIfAny();
        SubmittedInput.Insert(0, inputText);
    }

    private void RemoveExcessSubmittedInputIfAny()
    {
        while (SubmittedInput.Count > m_capacity)
            SubmittedInput.RemoveAt(SubmittedInput.Count - 1);
    }

    public new void Dispose()
    {
        base.Dispose();
        PerformDispose();
        GC.SuppressFinalize(this);
    }

    private void PerformDispose()
    {
        if (m_disposed)
            return;

        if (m_config != null)
        {
            m_config.Console.MaxMessages.OnChanged -= OnMaxMessagesChanged;

            // TODO: Investigate whether this is correct or not, the logger
            // documentation isn't clear and stack overflow has some unusual
            // results for how to properly remove the logger.
            // The logger stops logging to this target after we dispose of
            // this object, but I'd like to make sure that it's foolproof.
            RemoveLogger();
        }

        m_disposed = true;
    }
}
