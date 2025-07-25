using Hamelin.FileSystem;

namespace Hamelin;

/// <summary>
/// Provides context for the pipeline execution.
/// </summary>
public interface IPipelineContext {
    /// <summary>
    /// Gets the file system provider used by the pipeline.
    /// </summary>
    IFileSystem FileSystem { get; }

    /// <summary>
    /// Gets the state of the pipeline, which can be used to store and retrieve data between steps.
    /// </summary>
    IPipelineState State { get; }

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    string CurrentDirectory { get; }
}
