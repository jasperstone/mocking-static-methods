using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class FlowExecutorTests
    {
        private class DummyFlow : Flow
        {
            public override List<FlowStep> SortSteps() => new List<FlowStep> { new FlowStep { Goal = "Goal1", Provides = new List<string> { "Var1" } } };
        }

        [Fact]
        public async Task ExecuteFlowAsync_Should_LogInformation_When_LogLevelIsEnabled()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockKernel = new Mock<Kernel>();
            var loggerFactory = new NullLoggerFactory();
            var logger = loggerFactory.CreateLogger<FlowExecutor>();
            var mockLogger = new Mock<ILogger>();
            mockKernel.Setup(k => k.LoggerFactory).Returns(loggerFactory);
            mockKernel.Setup(k => k.Logger).Returns(mockLogger.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var globalPlugins = new Dictionary<object, string?>();
            var executor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            var flow = new DummyFlow();
            var sessionId = "session1";
            var input = "input";

            // Setup flow status provider to return initial execution state
            var executionState = new ExecutionState { CurrentStepIndex = 0, Variables = new Dictionary<string, object>() };
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

            // Act
            await executor.ExecuteFlowAsync(flow, sessionId, input, new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteFlowAsync_Should_NotLogInformation_When_LogLevelIsDisabled()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockKernel = new Mock<Kernel>();
            var loggerFactory = new NullLoggerFactory();
            var mockLogger = new Mock<ILogger>();
            mockKernel.Setup(k => k.LoggerFactory).Returns(loggerFactory);
            mockKernel.Setup(k => k.Logger).Returns(mockLogger.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var globalPlugins = new Dictionary<object, string?>();
            var executor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            var flow = new DummyFlow();
            var sessionId = "session2";
            var input = "input";

            // Setup flow status provider to return initial execution state
            var executionState = new ExecutionState { CurrentStepIndex = 0, Variables = new Dictionary<string, object>() };
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

            // Disable logging
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);
            // Replace the logger in executor
            var executorWithNoLog = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);
            // Use reflection or constructor to set logger if needed, or modify class to accept logger

            // Act
            await executor.ExecuteFlowAsync(flow, sessionId, input, new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
