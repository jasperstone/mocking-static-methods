using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var kernelMock = new Mock<Kernel>(MockBehavior.Strict, null, null, null);
            var config = new FlowOrchestratorConfig();

            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, config);

            var arguments = new KernelArguments();
            var question = "What is the answer?";
            var previousSteps = new List<ReActStep>();

            // Setup available functions to simulate multiple functions so it doesn't early return
            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction("Plugin1", "Func1", new List<Parameter>()),
                new KernelFunction("Plugin2", "Func2", new List<Parameter>())
            };

            // We need to mock GetAvailableFunctions and _reActFunction.InvokeAsync
            // But these are private/internal, so we will use reflection or create a derived test class to override them

            var reActEngineTest = new ReActEngineTestable(kernelMock.Object, loggerMock.Object, config)
            {
                AvailableFunctions = availableFunctions,
                LlmResponseText = "some response"
            };

            // Act
            var step = await reActEngineTest.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response : some response")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper derived class to override internal methods for testing
        private class ReActEngineTestable : ReActEngine
        {
            public List<KernelFunction> AvailableFunctions { get; set; } = new();
            public string LlmResponseText { get; set; } = string.Empty;

            public ReActEngineTestable(Kernel kernel, ILogger logger, FlowOrchestratorConfig config)
                : base(kernel, logger, config)
            {
            }

            protected override IEnumerable<KernelFunction> GetAvailableFunctions(Kernel kernel)
            {
                return AvailableFunctions;
            }

            protected override Task<SKContext> InvokeReActFunctionAsync(Kernel kernel, KernelArguments arguments)
            {
                var contextMock = new Mock<SKContext>();
                contextMock.Setup(c => c.GetValue<string>()).Returns(LlmResponseText);
                return Task.FromResult(contextMock.Object);
            }
        }
    }

    // Minimal stubs for dependencies to compile
    internal class Kernel
    {
        public Kernel(object a, object b, object c) { }
    }

    internal class KernelArguments : Dictionary<string, object> { }

    internal class ReActStep
    {
        public string? Action { get; set; }
        public string? FinalAnswer { get; set; }
        public string? Thought { get; set; }
        public string? Observation { get; set; }
        public Dictionary<string, string>? ActionVariables { get; set; }
    }

    internal class KernelFunction
    {
        public string PluginName { get; }
        public string Name { get; }
        public List<Parameter> Parameters { get; }

        public KernelFunction(string pluginName, string name, List<Parameter> parameters)
        {
            PluginName = pluginName;
            Name = name;
            Parameters = parameters;
        }
    }

    internal class Parameter
    {
        public string Name { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }
    }

    internal class SKContext
    {
        public virtual T? GetValue<T>() => default;
    }

    internal class FlowOrchestratorConfig
    {
        public List<string> ExcludedPlugins { get; } = new();
        public AIRequestSettings? AIRequestSettings { get; set; }
        public object? ReActPromptTemplateConfig { get; set; }
    }

    internal class AIRequestSettings
    {
        public string? ModelId { get; set; }
    }
}
