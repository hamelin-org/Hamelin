using System.Reflection;
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

    /// <summary>
    /// Registers the steps contained within the given assembly in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the steps to.</param>
    /// <param name="assembly">The assembly from which to add the discovered steps.</param>
    /// <returns>The updated service collection with the steps registered.</returns>
    public static IServiceCollection AddStepsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var stepType = typeof(IPipelineStep);
        var stepTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true } && stepType.IsAssignableFrom(t));

        foreach (var type in stepTypes)
        {
            services.AddTransient(type);
        }

        return services;
    }

    /// <summary>
    /// Registers the steps contained within the assembly that contains the given type.
    /// </summary>
    /// <param name="services">The service collection to add the steps to.</param>
    /// <typeparam name="TType">The type from which to get the assembly containing the steps.</typeparam>
    /// <returns>The updated service collection with the steps registered.</returns>
    public static IServiceCollection AddStepsFromAssemblyContaining<TType>(this IServiceCollection services)
    {
        var assembly = typeof(TType).Assembly;
        return services.AddStepsFromAssembly(assembly);
    }
}
