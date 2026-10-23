using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation_WhenStepIsCompleted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();

            var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, new Dictionary<object, string?>());

            var flow = new Flow();
            var sessionId = "testSessionId";
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
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
