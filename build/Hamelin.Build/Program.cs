using Hamelin;
using Microsoft.Extensions.Hosting;

var builder = PipelineApplication.CreateBuilder(args);

var pipeline = builder.Build();

pipeline.Run();
