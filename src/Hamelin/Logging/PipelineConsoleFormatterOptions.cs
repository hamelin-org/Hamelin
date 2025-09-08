using Microsoft.Extensions.Logging.Console;

namespace Hamelin.Logging;

/// <summary>
/// Options for the Hamelin console log formatter.
/// </summary>
public class PipelineConsoleFormatterOptions : ConsoleFormatterOptions
{
    /// <summary>
    /// Creates a new <see cref="PipelineConsoleFormatterOptions" /> with the default behaviours.
    /// </summary>
    public PipelineConsoleFormatterOptions()
    {
        TimestampFormat = "o"; // ISO 8601
        ColorBehavior = LoggerColorBehavior.Default;
        IncludeScopes = true;
        IncludeStepNames = true;
    }

    /// <summary>
    /// Determines when to use color when logging messages.
    /// </summary>
    public LoggerColorBehavior ColorBehavior { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether step names are included.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if step names are included.
    /// </value>
    public bool IncludeStepNames { get; set; }
}
