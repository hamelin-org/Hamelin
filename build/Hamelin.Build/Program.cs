using Hamelin;
using Hamelin.Build;
using Hamelin.Build.Services;
using Hamelin.Build.Steps;
using Hamelin.Runtimes.GitHubActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = PipelineApplication.CreateBuilder(args);

// Remove the default logger so we only get the GHA output.
builder.Logging
    .ClearProviders();

builder.Services
    .AddScoped<ICommandRunner, CliWrapCommandRunner>()
    .AddSingleton<IExternalScopeProvider, Esp>()
    .AddGitHubActionsRuntime()
    .AddStepsFromAssemblyContaining<Program>();

builder.Services.AddOptions<BuildOptions>()
    .BindConfiguration("Build")
    .Validate(b => !string.IsNullOrEmpty(b.ArtifactsDirectory))
    .Validate(b => !string.IsNullOrEmpty(b.TempDirectory))
    .Validate(b => !string.IsNullOrEmpty(b.Configuration))
    .Validate(b => !string.IsNullOrEmpty(b.ProjectFile))
    .ValidateOnStart();

var pipeline = builder.Build();

string? mode = builder.Configuration["Mode"];
switch (mode)
{
    case "PullRequest":
        pipeline
            .UseStep<CleanStep>()
            .UseStep<FormatStep>()
            .UseStep<ExtractProjectStep>()
            .UseStep<VersionStep>()
            .UseStep<RestoreStep>()
            .UseStep<BuildStep>()
            .UseStep<TestStep>();
        break;
    case "Release":
        pipeline
            .UseStep<CleanStep>()
            .UseStep<RestoreStep>()
            .UseStep<BuildStep>()
            .UseStep<PackStep>()
            .UseStep<PublishStep>();
        break;
    default:
        throw new InvalidOperationException($"Unknown mode: {mode}");
}

pipeline.Run();
