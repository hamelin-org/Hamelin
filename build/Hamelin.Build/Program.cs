using Hamelin;
using Hamelin.Build;
using Hamelin.Build.Services;
using Hamelin.Build.Steps;
using Hamelin.Runtimes.GitHubActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Version = Hamelin.Build.Steps.Version;

var builder = PipelineApplication.CreateBuilder(args);

builder.Services
    .AddScoped<ICommandRunner, CliWrapCommandRunner>()
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
            .UseStep<Clean>()
            .UseStep<Format>()
            .UseStep<ExtractProject>()
            .UseStep<Version>()
            .UseStep<Restore>()
            .UseStep<Build>()
            .UseStep<Test>();
        break;
    case "Release":
        pipeline
            .UseStep<Clean>()
            .UseStep<Restore>()
            .UseStep<Build>()
            .UseStep<Pack>()
            .UseStep<Publish>();
        break;
    default:
        throw new InvalidOperationException($"Unknown mode: {mode}");
}

pipeline.Run();
