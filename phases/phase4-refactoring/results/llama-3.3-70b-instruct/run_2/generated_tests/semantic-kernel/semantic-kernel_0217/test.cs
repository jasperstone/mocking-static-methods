using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution
{
    public class FlowExecutorTests
    {
        [Fact]
        public void LogInformation_Called_WhenExitingLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(null, null, null, null);
            flowExecutor._logger = loggerMock.Object;

            // Act
            var stepIndex = 1;
            var stepState = new ExecutionState.StepExecutionState();
            var step = new FlowStep("TestGoal", null);
            flowExecutor._logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, step.Goal);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
