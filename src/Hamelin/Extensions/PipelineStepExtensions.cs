using System.ComponentModel;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Hamelin;

/// <summary>
/// Provides extensions for working with <see cref="IPipelineStep"/>
/// </summary>
public static class PipelineStepExtensions
{
    /// <summary>
    /// Gets the display name of the step
    /// </summary>
    /// <param name="step">The step to get the name from</param>
    /// <returns>The contents of the <see cref="DisplayNameAttribute"/> on the step's class, or the class name if no display name is available.</returns>
    public static string GetDisplayName(this IPipelineStep step)
    {
        var displayNameAttribute = (DisplayNameAttribute?)step.GetType().GetCustomAttribute(typeof(DisplayNameAttribute));

        return displayNameAttribute?.DisplayName ?? step.GetType().Name;
    }
}
