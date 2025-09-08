using Hamelin.Logging;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Hamelin;

/// <summary>
/// Provides extension methods for registering console log formatters.
/// </summary>
public static class ConsoleFormatterExtensions
{
    /// <summary>
    /// Adds the pipeline friendly log formatter to the builder with the specified configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    public static ILoggingBuilder AddPipelineConsoleFormatter(
        this ILoggingBuilder builder,
        Action<PipelineConsoleFormatterOptions> configure) =>
        builder.AddConsole(options => options.FormatterName = PipelineConsoleFormatter.FormatterName)
            .AddConsoleFormatter<PipelineConsoleFormatter, PipelineConsoleFormatterOptions>(configure);

    /// <summary>
    /// Adds the pipeline friendly log formatter to the builder.
    /// </summary>
    /// <param name="builder"></param>
    public static ILoggingBuilder AddPipelineConsoleFormatter(this ILoggingBuilder builder) =>
        builder.AddConsole(options => options.FormatterName = PipelineConsoleFormatter.FormatterName)
            .AddConsoleFormatter<PipelineConsoleFormatter, PipelineConsoleFormatterOptions>();
}
