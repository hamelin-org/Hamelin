# Hamelin

Hamelin is an unopinionated library for creating CI/CD pipelines in .NET. It uses the .NET hosting model to provide a familiar way to create testable build and deployment pipelines while also being able to leverage core features of the .NET ecosystem, like `IConfiguration`, `IServiceProvider` and `ILogger`.

## Installation

Add the Hamelin NuGet package to your project using the .NET CLI:

```bash
dotnet add package Hamelin
```

## Usage

Hamelin uses the [.NET Generic Host model](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) which means it should be familiar to anyone who has used ASP.NET. It is typically recommended for use in a console application, but other application types can also be used.

The initial setup involves creating a new `PipelineApplicationBuilder`, configuring things like logging and dependency injection, and then building a `PipelineApplication`. Once the pipeline application has been built, steps can be registered in an order of your choice, before the pipeline is run.

Here’s a simple example of how to set up a pipeline, using some hypothetical steps like `Clean`, `Build`, and `RunTests`:

### Basic Example

```csharp
using Hamelin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = PipelineApplication.CreateBuilder(args);

builder.Services
    .AddStepsFromAssemblyContaining<Program>();

builder.Services
    .AddOptions<PipelineOptions>()
    .BindConfiguration("Pipeline");

var pipeline = builder.Build();

pipeline
    .UseStep<Clean>()
    .UseStep<Build>()
    .UseStep<RunTests>();

await pipeline.RunAsync();
```

Unlike a normal ASP.NET application, a Hamelin pipeline will terminate after the pipeline has been run, so the console application will exit after the `Run`/`RunAsync` method completes.

### Dependency Injection

Hamelin leverages the built-in dependency injection system in .NET. You can register your steps and any other services you need in the `IServiceCollection` during the setup phase. This allows you to inject dependencies into your pipeline steps, making them more modular and testable.

Before use, pipeline steps must be registered with the service collection. For ease of use, all steps in an assembly can be registered using the `AddStepsFromAssemblyContaining<T>()` method, which scans the specified assembly for classes that implement the `IPipelineStep` interface.

Each pipeline execution is run inside its own dependency scope, similar to a web request in ASP.NET, so state is not maintained between runs (unless explicitly captured in a singleton).

### Writing Steps

Steps in Hamelin are classes that implement the `IPipelineStep` interface. Steps are resolved from the `IServiceProvider` at runtime, so they support dependency injection through the constructor.

```csharps
public class Build(ILogger<Build> logger, IOptions<BuildOptions> options, ICommandRunner commands) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Building application...");
        await commands.Run(
            command: "dotnet",
            arguments: ["build", "--configuration", options.Value.Configuration],
            cancellationToken
        );
    }
}
```

### Pipeline Context

The `IPipelineContext` interface provides access to the current pipeline execution context, including the current directory, the file system, any state that has been captured in previous steps and the exit code. These are explained in more detail below.

### The File System

The `IFileSystem` interface is used to interact with the file system during pipeline execution. By default it interacts with the physical file system, but it can be replaced with a mock implementation for testing purposes.

It can be injected directly, or can be made accessible through `IPipelineContext.FileSystem`.

#### Example

```csharp
public class Publish(IOptions<BuildOptions> options, IPipelineContext context) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var packageFile = context.FileSystem.CurrentDirectory
            .GetDirectory(options.Value.ArtifactsDirectory)
            .GetFiles("*.nupkg")
            .Single();

        // TODO: Publish the package to a NuGet feed
    }
}
```

### Capturing State

`The IPipelineState` interface is used to capture and share state between steps in a pipeline. This allows steps to store data that can be accessed by subsequent steps without global variables. It is meant to be lightweight and simple to use/mock. If you need more complex state management, consider using a more robust solution.

It can be injected directly, or can be made accessible through `IPipelineContext.State`.

#### Example

```csharp
public class SetProjectInfo(IPipelineContext context) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var myState = new ProjectInfo()
        {
            Name = "MyProject",
            Version = "1.0.0"
        };
        context.State.Set(myState);
    }
}

public class GetProjectInfo(ILogger<StoreProjectInfo> logger, IPipelineContext context) : IPipelineStep
{
    public async Task Run(CancellationToken cancellationToken = default)
    {
        var state = context.State.Get<ProjectInfo>()
            ?? throw new InvalidOperationException("ProjectInfo not found in state.");

        logger.LogInformation("Stored project info: {Name} {Version}", state.Name, state.Version);
    }
}
```

### Logging

Hamelin uses the built-in logging system in .NET. You can configure logging during the setup phase using the `ILoggingBuilder` interface. This allows you to log messages from your pipeline steps, which can be useful for debugging and monitoring.

By default, Hamelin will log messages to the console using a custom formatter, but you can configure it to log to other providers like files, databases, or external services.

### Configuration

Hamelin supports configuration through the `IConfiguration` interface, allowing you to bind configuration settings to your pipeline as you would for other .NET apps. By default, environment variables, `appsettings.json` and `appsettings.{Environment}.json` are all loaded, just as they are in a typical ASP.NET Core application, but this behavior can be customized to load configuration from elsewhere, such as Azure App Configuration.

## Advanced Usage

### Exit Codes

Custom exit codes can be set for the pipeline using the `IPipelineContext.ExitCode` property. This allows you to indicate the success or failure of the pipeline execution.

When `PipelineExecutionOptions.EnableAutomaticExitCodes` is set to `true` (the default), Hamelin will automatically set the exit code based on the success or failure of the pipeline steps. If a step fails, the exit code will be set to a non-zero value, based on the termination mode, indicating an error. If all steps succeed, the exit code will be set to zero, or use whatever value has been set in `IPipelineContext.ExitCode`.

When `PipelineExecutionOptions.EnableAutomaticExitCodes` is set to `false`, the value in `IPipelineContext.ExitCode` will always be used, defaulting to `0` if it is `null`.

### Termination Modes

By default, Hamelin will stop executing steps as soon as one of them fails, and the exception will be propagated, but this behavior can be customized using the `PipelineExecutionOptions.TerminationMode` property. When set to `PipelineTerminationMode.StopAfterAllSteps`, the pipeline will continue executing steps, and any unhandled errors will be logged.

### Dependency Scopes

Each pipeline execution is run inside its own dependency scope, similar to a web request in ASP.NET. This means that services registered as `Scoped` will be created and disposed of for each pipeline run, while `Singleton` services will be shared across all runs. This allows you to maintain state within a single pipeline execution without affecting other executions in advanced cases where a pipeline is run multiple times.

### Hooks

#### Pipeline Hooks

Sometimes there are actions you want to run before and after a pipeline that don't quite work as a step. An example of this is publishing a summary at the end of a pipeline, regardless of whether the pipeline succeeded or not.

For these cases, there are a set of hooks that are run automatically at the start and end of every pipeline. They can be added to by registering instances of `IPrePipelineHook` and `IPostPipelineHook` with the `IServiceCollection`.

Hooks are run serially in the order they are registered with the service provider. Any unhandled exceptions will be logged, but won't cause the pipeline to terminate.

Post-pipeline hooks will be run regardless of any errors that occur during the pipeline steps.

#### Step Hooks

In some niche use cases, it can also be useful to run hooks before and after each step. Step hooks work the same as pipeline hooks, needing to be registered with the `IServiceCollection` using the `IPreStepHook` and `IPostStepHook` interfaces.

Step hooks are run serially in the order they are registered with the service provider. Any unhandled exceptions will be logged, but won't cause the pipeline to terminate.

Post-step hooks will be run regardless of any errors that occur during the pipeline step.

One example use case for this is when integrating to a build system, and being able to group the log output for each step.
