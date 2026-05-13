using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation_WhenExitLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            // Setup a Flow with one step that will trigger the exit loop log
            var flow = new Flow("TestFlow");
            var step = new FlowStep("Step1", "Goal1");
            step.Provides.Add("output");
            flow.AddStep(step);

            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables = new Dictionary<string, string>();
            executionState.StepStates = new Dictionary<string, ExecutionState.StepExecutionState>();

            flowStatusProviderMock.Setup(f => f.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            kernelBuilderMock.Setup(k => k.Build())
                .Returns(new KernelStub(loggerMock.Object));

            var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection);

            // Act
            // We simulate the exit loop by invoking ExecuteFlowAsync and forcing the stepResult to have TryGetExitLoopResponse true.
            // Since the actual method is complex and private, we simulate by calling ExecuteFlowAsync and verifying the logger call.
            // This is a limitation due to the complexity of the method and dependencies.
            await flowExecutor.ExecuteFlowAsync(flow, "session1", "input", new KernelArguments());

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exiting loop for step")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        // Stub Kernel to inject logger
        private class KernelStub : Kernel
        {
            private readonly ILogger _logger;

            public KernelStub(ILogger logger)
            {
                _logger = logger;
            }

            public override ILoggerFactory LoggerFactory => new LoggerFactoryStub(_logger);
        }

        private class LoggerFactoryStub : ILoggerFactory
        {
            private readonly ILogger _logger;

            public LoggerFactoryStub(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }
    }
}
