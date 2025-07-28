using Hamelin;
using Hamelin.Build;
using Hamelin.Build.Services;
using Hamelin.Build.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = PipelineApplication.CreateBuilder(args);

builder.Services
    .AddScoped<ICommandRunner, CliWrapCommandRunner>()
    .AddStepsFromAssemblyContaining<Program>();

builder.Services.AddOptions<BuildOptions>()
    .BindConfiguration("Build")
    .Validate(b => !string.IsNullOrEmpty(b.ArtifactsDirectory))
    .Validate(b => !string.IsNullOrEmpty(b.TempDirectory))
    .Validate(b => !string.IsNullOrEmpty(b.Configuration))
    .ValidateOnStart();

var pipeline = builder.Build();

pipeline
    .UseStep<CleanStep>()
    .UseStep<FormatStep>()
    .UseStep<RestoreStep>()
    .UseStep<BuildStep>()
    .UseStep<TestStep>();

pipeline.Run();
