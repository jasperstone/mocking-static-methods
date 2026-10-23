using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution
{
    public class FlowExecutorTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(null, null, null, null);
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 2 };
            var stepGoal = "TestGoal";

            // Act
            flowExecutor._logger.LogInformation("Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, stepGoal);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
