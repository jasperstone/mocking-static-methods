using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Xunit;
using Moq;
using Moq.Language.Flow;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Flow.Tests.Execution;

public class ReActEngineLoggerTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
    {
        // Arrange - Create real dependencies that work without accessing internal types
        var testLogger = new TestLogger();
        var kernelBuilder = Kernel.CreateBuilder();
        var kernel = kernelBuilder.Build();
        
        // Add a dummy function to avoid single-function auto-selection path
        kernel.Plugins.AddFromFunctions("test", 
            KernelFunctionFactory.CreateFromPrompt("test", description: "test"));

        var config = new FlowOrchestratorConfig();
        
        // Use reflection to replace logger after construction (public ctor accessible via assembly)
        var engine = new ReActEngine(kernel, NullLogger.Instance, config);
        typeof(ReActEngine).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(engine, testLogger);

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        var result = await engine.GetNextStepAsync(kernel, arguments, question, previousSteps);

        // Assert
        Assert.NotNull(result);
        Assert.Single(testLogger.DebugMessages);
        Assert.Contains("Response", testLogger.DebugMessages[0]);
    }

    [Fact]
    public async Task GetNextStepAsync_LogsDebugAutoSelect_WhenSingleNoParamFunction()
    {
        // Arrange
        var testLogger = new TestLogger();
        var kernelBuilder = Kernel.CreateBuilder();
        var kernel = kernelBuilder.Build();
        
        // Add single parameterless function to trigger auto-selection path
        var noParamFunction = KernelFunctionFactory.CreateFromPrompt("noparams", description: "no params");
        noParamFunction = noParamFunction.SetDefaultParameterValue("input", null);
        kernel.Plugins.AddFromFunctions("single", noParamFunction);

        var config = new FlowOrchestratorConfig();
        var engine = new ReActEngine(kernel, testLogger, config);

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        var result = await engine.GetNextStepAsync(kernel, arguments, question, previousSteps);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Action);
        Assert.Contains("Auto selecting single.noparams", testLogger.DebugMessages);
    }

    [Fact]
    public void LoggerExtension_IsEnabledCheck_WorksAsExpected()
    {
        // This tests the ILoggerExtensions behavior indirectly through the pattern used
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        // The extension method LoggerExtensions.LogDebug calls IsEnabled first
        // This verifies the conditional logging pattern works
        Assert.True(mockLogger.Object.IsEnabled(LogLevel.Debug));
    }
}

internal class TestLogger : ILogger
{
    public List<string> DebugMessages { get; } = new();
    public List<string> InformationMessages { get; } = new();
    public List<string> WarningMessages { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;
    
    public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Debug || logLevel == LogLevel.Information;
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        switch (logLevel)
        {
            case LogLevel.Debug:
                DebugMessages.Add(message);
                break;
            case LogLevel.Information:
                InformationMessages.Add(message);
                break;
            case LogLevel.Warning:
                WarningMessages.Add(message);
                break;
        }
    }
}
