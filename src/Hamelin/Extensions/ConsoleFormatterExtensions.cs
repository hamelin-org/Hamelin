using Hamelin.Logging;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Hamelin;

/// <summary>
/// Provides extension methods for registering console log formatters.
/// </summary>
public static class ConsoleFormatterExtensions
{
    /// <param name="builder"></param>
    extension(ILoggingBuilder builder)
    {
        /// <summary>
        /// Adds the pipeline friendly log formatter to the builder with the specified configuration.
        /// </summary>
        /// <param name="configure">A delegate to configure the formatter.</param>
        public ILoggingBuilder AddPipelineConsoleFormatter(Action<PipelineConsoleFormatterOptions> configure) =>
            builder.AddConsole(options => options.FormatterName = PipelineConsoleFormatter.FormatterName)
                .AddConsoleFormatter<PipelineConsoleFormatter, PipelineConsoleFormatterOptions>(configure);

        /// <summary>
        /// Adds the pipeline friendly log formatter to the builder.
        /// </summary>
        public ILoggingBuilder AddPipelineConsoleFormatter() =>
            builder.AddConsole(options => options.FormatterName = PipelineConsoleFormatter.FormatterName)
                .AddConsoleFormatter<PipelineConsoleFormatter, PipelineConsoleFormatterOptions>();
    }
}
