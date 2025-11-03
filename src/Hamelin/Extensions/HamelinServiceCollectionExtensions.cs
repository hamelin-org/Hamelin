using System.Reflection;
using Hamelin.Hooks;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Hamelin;

/// <summary>
/// Provides extension methods for registering pipeline steps in the service collection.
/// </summary>
public static class HamelinServiceCollectionExtensions
{
    /// <summary>
    /// Registers a step in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the step to.</param>
    /// <typeparam name="TPipelineStep">The type of the pipeline step to register. It must implement <see cref="IPipelineStep"/>.</typeparam>
    /// <returns>The updated service collection with the step registered.</returns>
    public static IServiceCollection AddStep<TPipelineStep>(this IServiceCollection services)
        where TPipelineStep : class, IPipelineStep
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

    /// <summary>
    /// Registers a pre-pipeline hook in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the hook to.</param>
    /// <typeparam name="THook">The type of the pre-pipeline hook to register. It must implement <see cref="IPrePipelineHook"/>.</typeparam>
    /// <returns>The updated service collection with the hook registered.</returns>
    public static IServiceCollection AddPrePipelineHook<THook>(this IServiceCollection services)
        where THook : class, IPrePipelineHook
    {
        return services.AddTransient<IPrePipelineHook, THook>();
    }

    /// <summary>
    /// Registers a post-pipeline hook in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the hook to.</param>
    /// <typeparam name="THook">The type of the pre-pipeline hook to register. It must implement <see cref="IPostPipelineHook"/>.</typeparam>
    /// <returns>The updated service collection with the hook registered.</returns>
    public static IServiceCollection AddPostPipelineHook<THook>(this IServiceCollection services)
        where THook : class, IPostPipelineHook
    {
        return services.AddTransient<IPostPipelineHook, THook>();
    }

    /// <summary>
    /// Registers a pre-step hook in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the hook to.</param>
    /// <typeparam name="THook">The type of the pre-pipeline hook to register. It must implement <see cref="IPreStepHook"/>.</typeparam>
    /// <returns>The updated service collection with the hook registered.</returns>
    public static IServiceCollection AddPreStepHook<THook>(this IServiceCollection services)
        where THook : class, IPreStepHook
    {
        return services.AddTransient<IPreStepHook, THook>();
    }

    /// <summary>
    /// Registers a post-step hook in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the hook to.</param>
    /// <typeparam name="THook">The type of the pre-pipeline hook to register. It must implement <see cref="IPostStepHook"/>.</typeparam>
    /// <returns>The updated service collection with the hook registered.</returns>
    public static IServiceCollection AddPostStepHook<THook>(this IServiceCollection services)
        where THook : class, IPostStepHook
    {
        return services.AddTransient<IPostStepHook, THook>();
    }

    /// <summary>
    /// Registers the hooks contained within the given assembly in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the hooks to.</param>
    /// <param name="assembly">The assembly from which to add the discovered hooks.</param>
    /// <returns>The updated service collection with the hooks registered.</returns>
    public static IServiceCollection AddHooksFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var allTypes = assembly.GetTypes();
        AddHooksOfType(typeof(IPrePipelineHook));
        AddHooksOfType(typeof(IPostPipelineHook));
        AddHooksOfType(typeof(IPreStepHook));
        AddHooksOfType(typeof(IPostStepHook));

        return services;

        void AddHooksOfType(Type hookType)
        {
            var hookTypes = allTypes
                .Where(t => t is { IsAbstract: false, IsClass: true } && hookType.IsAssignableFrom(t));
            foreach (var type in hookTypes)
            {
                services.AddTransient(hookType, type);
            }
        }
    }

    /// <summary>
    /// Registers the hooks contained within the assembly that contains the given type.
    /// </summary>
    /// <param name="services">The service collection to add the hooks to.</param>
    /// <typeparam name="TType">The type from which to get the assembly containing the hooks.</typeparam>
    /// <returns>The updated service collection with the hooks registered.</returns>
    public static IServiceCollection AddHooksFromAssemblyContaining<TType>(this IServiceCollection services)
    {
        var assembly = typeof(TType).Assembly;
        return services.AddHooksFromAssembly(assembly);
    }
}
