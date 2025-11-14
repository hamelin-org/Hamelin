using System.ComponentModel;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Hamelin;

/// <summary>
/// Provides extensions for working with <see cref="IPipelineStep"/>
/// </summary>
public static class PipelineStepExtensions
{
    /// <param name="step"></param>
    extension(IPipelineStep step)
    {
        /// <summary>
        /// Gets the display name of the step
        /// </summary>
        /// <returns>The contents of the <see cref="DisplayNameAttribute"/> on the step's class, or the class name if no display name is available.</returns>
        public string GetDisplayName()
        {
            var displayNameAttribute = step.GetType().GetCustomAttribute<DisplayNameAttribute>();
            return displayNameAttribute?.DisplayName ?? step.GetType().Name;
        }
    }
}
