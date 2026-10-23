using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FlowExecutorTests
{
    public class LogInformationTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_Should_LogInformation_When_ExitingLoop()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<IKernel>();
            var mockSystemKernel = new Mock<Kernel>();
            var mockReActEngine = new Mock<ReActEngine>();

            // Setup logger factory to return our mock logger
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            // Setup kernel builder to return our mock kernel
            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            // Instantiate FlowExecutor with mocks
            var globalPlugins = new Dictionary<object, string?>();
            var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            // Create a dummy flow
            var flow = new Mock<Flow>();
            flow.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { new FlowStep { /* initialize as needed */ } });
            flow.Setup(f => f.Name).Returns("TestFlow");

            // Setup execution state to trigger the exit loop branch
            var executionState = new Mock<ExecutionState>();
            executionState.Setup(es => es.CurrentStepIndex).Returns(0);
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(new ExecutionState());

            // Act
            await flowExecutor.ExecuteFlowAsync(flow.Object, "sessionId", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exiting loop for step")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
