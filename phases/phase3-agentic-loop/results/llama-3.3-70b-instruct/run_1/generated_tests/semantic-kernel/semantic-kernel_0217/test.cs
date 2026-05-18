using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public void LogInformation_WhenExitingLoop_WithStepIndexIterationAndGoal()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var flowOrchestratorConfig = new FlowOrchestratorConfig();
            var flowExecutor = new FlowExecutor(
                kernelBuilderMock.Object,
                flowStatusProviderMock.Object,
                new Dictionary<object, string?>(),
                flowOrchestratorConfig
            );
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var iteration = 2;
            var goal = "TestGoal";

            // Act
            flowExecutor._logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, goal);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.",
                    stepIndex,
                    iteration,
                    goal
                ),
                Times.Once
            );
        }
    }
}
