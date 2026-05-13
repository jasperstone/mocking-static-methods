using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsStepCompletion()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>();
            var mockReActEngine = new Mock<ReActEngine>();

            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);
            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var flowExecutor = new FlowExecutor(
                mockKernelBuilder.Object,
                mockFlowStatusProvider.Object,
                new Dictionary<object, string?>(),
                new FlowOrchestratorConfig());

            var flow = new Flow();
            var sessionId = "testSession";
            var input = "testInput";
            var kernelArguments = new KernelArguments();

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };

            mockFlowStatusProvider.Setup(fsp => fsp.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
