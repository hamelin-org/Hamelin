using Hamelin;
using Hamelin.Build;
using Hamelin.Build.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = PipelineApplication.CreateBuilder(args);

builder.Services.AddStepsFromAssemblyContaining<Program>();

builder.Services.AddOptions<BuildOptions>()
    .BindConfiguration("Build")
    .Validate(b => !string.IsNullOrEmpty(b.ArtifactsDirectory))
    .Validate(b => !string.IsNullOrEmpty(b.TempDirectory))
    .ValidateOnStart();

var pipeline = builder.Build();

pipeline.UseStep<CleanStep>();

pipeline.Run();
