using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

public class ReActEngineLoggerTests
{
    private class CapturingLogger : ILogger
    {
        public List<string> DebugMessages { get; } = new();
        public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Debug;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Debug)
            {
                DebugMessages.Add(formatter(state, exception));
            }
        }
    }

    private class DisabledLogger : ILogger
    {
        public List<string> DebugMessages { get; } = new();
        public bool IsEnabled(LogLevel logLevel) => false;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    [Fact]
    public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
    {
        // Arrange
        var logger = new CapturingLogger();
        var kernel = Mock.Of<Kernel>();
        var config = new FlowOrchestratorConfig();
        
        // Create engine using reflection since constructor is internal
        var engine = CreateEngine(kernel, logger, config);

        SetupReActFunction(engine, "test response");

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<object>();

        // Act
        var result = await InvokeGetNextStepAsync(engine, kernel, arguments, question, previousSteps);

        // Assert
        Assert.NotNull(result);
        Assert.Single(logger.DebugMessages);
        Assert.Contains("Response : test response", logger.DebugMessages[0]);
    }

    [Fact]
    public async Task GetNextStepAsync_DoesNotLogDebugResponse_WhenDebugDisabled()
    {
        // Arrange
        var logger = new DisabledLogger();
        var kernel = Mock.Of<Kernel>();
        var config = new FlowOrchestratorConfig();
        
        var engine = CreateEngine(kernel, logger, config);
        SetupReActFunction(engine, "test response");

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<object>();

        // Act
        var result = await InvokeGetNextStepAsync(engine, kernel, arguments, question, previousSteps);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(logger.DebugMessages);
    }

    private static object CreateEngine(Kernel kernel, ILogger logger, FlowOrchestratorConfig config)
    {
        var constructor = typeof(ReActEngine).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null, new[] { typeof(Kernel), typeof(ILogger), typeof(FlowOrchestratorConfig) }, null)!;
        return constructor.Invoke(new object[] { kernel, logger, config });
    }

    private static void SetupReActFunction(object engine, string response)
    {
        var mockFunction = new Mock<KernelFunction>();
        mockFunction.Setup(f => f.InvokeAsync(
            It.IsAny<Kernel>(), 
            It.IsAny<KernelArguments>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new KernelResult(response));
        
        var field = typeof(ReActEngine).GetField("_reActFunction", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(engine, mockFunction.Object);
    }

    private static Task<object?> InvokeGetNextStepAsync(object engine, Kernel kernel, KernelArguments arguments, 
        string question, List<object> previousSteps)
    {
        var method = typeof(ReActEngine).GetMethod("GetNextStepAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (Task<object?>)method.Invoke(engine, new object[] { kernel, arguments, question, previousSteps })!;
    }
}
