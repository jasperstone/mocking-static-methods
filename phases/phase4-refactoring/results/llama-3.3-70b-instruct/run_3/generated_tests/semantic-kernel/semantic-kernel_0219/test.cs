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
            var step = new FlowStep("TestGoal", null);

            // Act
            //flowExecutor.ExecuteFlowAsync(null, null, null, null); // This will call LogInformation

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains($"Completed step {stepIndex} for iteration={stepState.ExecutionCount}, goal={step.Goal}."))), Times.Once);
        }
    }
}
