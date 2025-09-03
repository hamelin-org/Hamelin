using System.Diagnostics.CodeAnalysis;
using System.Net;
using Hamelin.Internal;
using Hamelin.Logging;
using Hamelin.Tests.Unit.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Hamelin.Tests.Unit.Logging;

[Collection("Console")]
public class PipelineConsoleFormatterTests
{
    private readonly StringWriter _writer = new();
    private readonly FakeTimeProvider _timeProvider = new(DateTimeOffset.UtcNow);
    private ILogger<PipelineConsoleFormatterTests> _logger;
    private ConsoleLoggerProvider _provider;

    public PipelineConsoleFormatterTests()
    {
        Console.SetOut(_writer);
        ReplaceLoggerAndProvider(new PipelineConsoleFormatterOptions());
    }

    [MemberNotNull(nameof(_logger))]
    [MemberNotNull(nameof(_provider))]
    private void ReplaceLoggerAndProvider(PipelineConsoleFormatterOptions options)
    {
        _provider = CreateLoggerProvider(options);
        _logger = CreateLogger<PipelineConsoleFormatterTests>();
    }

    private ConsoleLoggerProvider CreateLoggerProvider(PipelineConsoleFormatterOptions formatterOptions)
    {
        var formatterOptionsMonitor = Substitute.For<IOptionsMonitor<PipelineConsoleFormatterOptions>>();
        formatterOptionsMonitor.CurrentValue.Returns(formatterOptions);

        var formatter = new PipelineConsoleFormatter(formatterOptionsMonitor, _timeProvider);

        var loggerOptionsMonitor = Substitute.For<IOptionsMonitor<ConsoleLoggerOptions>>();
        var loggerOptions = new ConsoleLoggerOptions { FormatterName = PipelineConsoleFormatter.FormatterName };
        loggerOptionsMonitor.CurrentValue.Returns(loggerOptions);

        return new ConsoleLoggerProvider(loggerOptionsMonitor, [formatter]);
    }

    private ILogger<T> CreateLogger<T>() =>
        LoggerFactory
            .Create(l =>
            {
                l.SetMinimumLevel(LogLevel.Trace);
                l.Services.AddSingleton<TimeProvider>(_timeProvider);
                l.ClearProviders();
                l.AddProvider(_provider);
            })
            .CreateLogger<T>();

    [Fact]
    public void LogCritical_TextOnly_LogsError()
    {
        // Arrange

        // Act
        _logger.LogCritical("Test Critical");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Critical);
        output.ShouldContain("Test Critical");
    }

    [Fact]
    public void LogError_TextOnly_LogsError()
    {
        // Arrange

        // Act
        _logger.LogError("Test Error");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Error);
        output.ShouldContain("Test Error");
    }

    [Fact]
    public void LogError_WithException_LogsErrorWithExceptionDetails()
    {
        // Arrange
        var ex = new Exception("Test Exception");

        // Act
        _logger.LogError(ex, "Test Error");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Error);
        output.ShouldContain("Test Error");
        output.ShouldContain("System.Exception: Test Exception");
    }

    [Fact]
    public void LogWarning_TextOnly_LogsWarning()
    {
        // Arrange

        // Act
        _logger.LogWarning("Test Warning");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Warning);
        output.ShouldContain("Test Warning");
    }

    [Fact]
    public void LogInformation_TextOnly_LogsInformation()
    {
        // Arrange

        // Act
        _logger.LogInformation("Test Information");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Information);
        output.ShouldContain("Test Information");
    }

    [Fact]
    public void LogDebug_TextOnly_LogsDebug()
    {
        // Arrange

        // Act
        _logger.LogDebug("Test Debug");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Debug);
        output.ShouldContain("Test Debug");
    }

    [Fact]
    public void LogTrace_TextOnly_LogsTrace()
    {
        // Arrange

        // Act
        _logger.LogTrace("Test Trace");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain(LogMessageLevels.Trace);
        output.ShouldContain("Test Trace");
    }

    [Theory]
    [InlineData("o")]
    [InlineData("dd MMM HH:mm:ss")]
    [InlineData("r")]
    public void Log_WithTimeFormatting_MatchesSystemTimeFormatting(string format)
    {
        // Arrange
        ReplaceLoggerAndProvider(new PipelineConsoleFormatterOptions { UseUtcTimestamp = true, TimestampFormat = format });

        // Act
        _logger.LogInformation("Test");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        string expectedFormattedTime = _timeProvider.GetUtcNow().ToString(format);
        output.ShouldContain(expectedFormattedTime);
    }

    [Fact]
    public void Log_WithNullTimeFormat_ShouldOmitTimestamp()
    {
        // Arrange
        ReplaceLoggerAndProvider(new PipelineConsoleFormatterOptions { TimestampFormat = null, ColorBehavior = LoggerColorBehavior.Disabled });

        // Act
        _logger.LogInformation("Test");
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        // Testing for the absence of something that can be in any format is tricky
        // This test is sensitive to other format changes, it would be nice to find a more reliable test
        output.ShouldStartWith(LogMessageLevels.Information);
    }

    [Fact]
    public async Task Log_ForPipelineStep_ShouldIncludeStepName()
    {
        // Arrange
        var formattingOptions = new PipelineConsoleFormatterOptions { ColorBehavior = LoggerColorBehavior.Disabled, IncludeScopes = true, IncludeStepNames = true };
        ReplaceLoggerAndProvider(formattingOptions);

        var testStep = new TestStep(CreateLogger<TestStep>());

        var pipelineRunner = DefaultPipelineRunnerHelpers.CreateRunner(
            steps: [testStep],
            logger: CreateLogger<DefaultPipelineRunner>(),
            stepRunner: DefaultPipelineStepRunnerHelpers.CreateRunner(logger: CreateLogger<DefaultPipelineStepRunner>())
        );

        // Act
        _ = await pipelineRunner.RunPipeline(TestContext.Current.CancellationToken);
        _provider.Dispose();

        // Assert
        string output = _writer.ToString();
        output.ShouldContain($"TestStep: {TestStep.TestMessage}");
    }
}

internal class TestStep(ILogger<TestStep> logger) : IPipelineStep
{
    public const string TestMessage = "Step 'Run' method called";

    public Task Run(CancellationToken cancellationToken = default)
    {
        logger.LogInformation(TestMessage);
        return Task.CompletedTask;
    }
}
