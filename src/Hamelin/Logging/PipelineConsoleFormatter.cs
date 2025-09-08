using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Hamelin.Logging;

/// <summary>
/// Formats CI/CD log entries in a concise and readable way.
/// </summary>
public class PipelineConsoleFormatter : ConsoleFormatter, IDisposable
{
    internal const string FormatterName = "HamelinConsole";

    private readonly IDisposable? _optionsReloadToken;
    private readonly TimeProvider _time;

    /// <summary>
    /// Initializes a new instance of <see cref="PipelineConsoleFormatter" />
    /// </summary>
    public PipelineConsoleFormatter(IOptionsMonitor<PipelineConsoleFormatterOptions> options, TimeProvider timeProvider) : base(FormatterName)
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        _time = timeProvider;
    }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(PipelineConsoleFormatterOptions options)
    {
        FormatterOptions = options;
    }

    /// <inheritdoc cref="IDisposable" />
    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    internal PipelineConsoleFormatterOptions FormatterOptions { get; set; }

    /// <summary>
    /// Writes the log message to the specified TextWriter
    /// </summary>
    /// <param name="logEntry"></param>
    /// <param name="scopeProvider"></param>
    /// <param name="textWriter"></param>
    /// <typeparam name="TState"></typeparam>
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        string? message = logEntry.Formatter(logEntry.State, logEntry.Exception);

        if (logEntry.Exception is null && string.IsNullOrEmpty(message))
        {
            return;
        }

        DateTimeOffset logTime = GetCurrentDateTime();

        string? category = GetStepName(scopeProvider);

        WriteMessage(textWriter, message, logTime, logEntry.LogLevel, category);

        if (logEntry.Exception is not null)
        {
            WriteMessage(textWriter, logEntry.Exception.ToString(), logTime, logEntry.LogLevel, category);
        }
    }

    private string? GetStepName(IExternalScopeProvider? scopeProvider)
    {
        if (scopeProvider is null || !FormatterOptions.IncludeScopes || !FormatterOptions.IncludeStepNames)
        {
            return null;
        }

        string? stepName = null;

        scopeProvider.ForEachScope((scope, state) =>
        {
            if (scope is not LogAttributes logAttributes)
            {
                return;
            }

            stepName = logAttributes.StepName;
        }, stepName);

        return stepName;
    }

    private void WriteMessage(TextWriter textWriter, string message, DateTimeOffset logTime, LogLevel logLevel, string? stepName)
    {
        string[] messageLines = message.Split(Environment.NewLine);
        foreach (string messageLine in messageLines)
        {
            WriteMessageLine(textWriter, messageLine, logTime, logLevel, stepName);
        }
    }

    private void WriteMessageLine(TextWriter textWriter, string message, DateTimeOffset logTime, LogLevel logLevel, string? stepName)
    {
        ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel);
        string logLevelString = GetLogLevelString(logLevel);

        string? timestamp = null;
        string? timestampFormat = FormatterOptions.TimestampFormat;
        if (timestampFormat != null)
        {
            timestamp = logTime.ToString(timestampFormat);
            textWriter.Write(' ');
        }

        if (!string.IsNullOrEmpty(timestamp))
        {
            textWriter.Write(timestamp);
            textWriter.Write(' ');
        }

        if (!string.IsNullOrEmpty(logLevelString))
        {
            textWriter.WriteColoredMessage(logLevelString, logLevelColors.Background, logLevelColors.Foreground);
            textWriter.Write(' ');
        }

        if (stepName is not null)
        {
            textWriter.Write(stepName);
            textWriter.Write(": ");
        }

        textWriter.WriteLine(message);
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        if (FormatterOptions.TimestampFormat is null)
        {
            return DateTimeOffset.MinValue;
        }

        return FormatterOptions.UseUtcTimestamp ? _time.GetUtcNow() : _time.GetLocalNow();
    }

    private static string GetLogLevelString(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.None => string.Empty,
            LogLevel.Trace => LogMessageLevels.Trace,
            LogLevel.Debug => LogMessageLevels.Debug,
            LogLevel.Information => LogMessageLevels.Information,
            LogLevel.Warning => LogMessageLevels.Warning,
            LogLevel.Error => LogMessageLevels.Error,
            LogLevel.Critical => LogMessageLevels.Critical,
            _ => $"[LEVEL {logLevel}]"
        };

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        if (FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled)
        {
            return new ConsoleColors(null, null);
        }

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Red, ConsoleColor.Black),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new ConsoleColors(null, null)
        };
    }

    private readonly struct ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
    {
        public ConsoleColor? Foreground { get; } = foreground;

        public ConsoleColor? Background { get; } = background;
    }
}
