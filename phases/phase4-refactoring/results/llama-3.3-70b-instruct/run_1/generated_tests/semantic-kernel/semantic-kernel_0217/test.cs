using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public void LogInformation_Called_WhenExitingLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(
                Mock.Of<IKernelBuilder>(),
                Mock.Of<IFlowStatusProvider>(),
                new Dictionary<object, string?>(),
                new FlowOrchestratorConfig());

            // Act
            flowExecutor._logger = loggerMock.Object;
            // Simulate exiting loop
            var stepResult = new FunctionResult(
                Mock.Of<KernelFunction>(),
                new object(),
                new Dictionary<string, object>());
            var stepState = new ExecutionState.StepExecutionState();
            var step = new FlowStep("TestStep", null);
            var stepIndex = 0;
            var iteration = 0;
            var goal = "TestGoal";

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
