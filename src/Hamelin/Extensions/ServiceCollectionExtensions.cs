using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Extensions;

/// <summary>
/// Provides extension methods for registering pipeline steps in the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a step in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the step to.</param>
    /// <typeparam name="TPipelineStep">The type of the pipeline step to register. It must implement <see cref="IPipelineStep"/>.</typeparam>
    /// <returns>The updated service collection with the step registered.</returns>
    public static IServiceCollection AddStep<TPipelineStep>(this IServiceCollection services) where TPipelineStep : class, IPipelineStep
    {
        return services.AddTransient<TPipelineStep>();
    }
}
