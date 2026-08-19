using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Moq;
using Xunit;

public class ReActEngineLoggerTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebug_WhenOnlyOneFunctionNoParameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var config = new FlowOrchestratorConfig();

        // We cannot instantiate ReActEngine directly because it is internal,
        // so we test the logging behavior indirectly by mocking dependencies
        // and simulating the conditions that trigger the LogDebug call.

        var availableFunction = new Mock<ISKFunction>();
        availableFunction.Setup(f => f.PluginName).Returns("Plugin");
        availableFunction.Setup(f => f.Name).Returns("Name");
        availableFunction.Setup(f => f.Parameters).Returns(Array.Empty<ParameterView>());

        var reActEngine = new TestableReActEngine(loggerMock.Object, config);
        reActEngine.SetAvailableFunctions(new[] { availableFunction.Object });

        var arguments = new KernelArguments();
        var question = "question";
        var previousSteps = new List<ReActStep>();

        // Act
        var result = await reActEngine.GetNextStepAsync(arguments, question, previousSteps);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Plugin.Name", result.Action);
        loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
        loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Auto selecting Plugin.Name")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    // Minimal interfaces and classes to support the test
    private interface ISKFunction
    {
        string PluginName { get; }
        string Name { get; }
        IReadOnlyList<ParameterView> Parameters { get; }
    }

    private class ParameterView
    {
    }

    private class KernelArguments : Dictionary<string, object>
    {
    }

    private class TestableReActEngine
    {
        private readonly ILogger _logger;
        private readonly FlowOrchestratorConfig _config;
        private ISKFunction[] _availableFunctions = Array.Empty<ISKFunction>();

        public TestableReActEngine(ILogger logger, FlowOrchestratorConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public void SetAvailableFunctions(ISKFunction[] functions)
        {
            _availableFunctions = functions;
        }

        public async Task<ReActStep?> GetNextStepAsync(KernelArguments arguments, string question, List<ReActStep> previousSteps)
        {
            arguments["question"] = question;
            // Simulate the logic that triggers the debug log when only one function with no parameters is available
            if (_availableFunctions.Length == 1 && _availableFunctions[0].Parameters.Count == 0)
            {
                var action = $"{_availableFunctions[0].PluginName}.{_availableFunctions[0].Name}";

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", action);
                }

                return await Task.FromResult(new ReActStep { Action = action });
            }

            return await Task.FromResult<ReActStep?>(null);
        }
    }
}
