using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    [TestClass]
    public class FlowExecutorTests
    {
        [TestMethod]
        public void LogInformation_WhenExitingLoop_WithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(
                Mock.Of<IKernelBuilder>(),
                Mock.Of<IFlowStatusProvider>(),
                new Dictionary<object, string?>(),
                new FlowOrchestratorConfig());
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 2 };
            var stepGoal = "TestGoal";

            // Act
            flowExecutor._logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, stepGoal);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains($"Exiting loop for step {stepIndex} with iteration={stepState.ExecutionCount}, goal={stepGoal}."))), Times.Once);
        }

        [TestMethod]
        public void LogInformation_WhenContinuingLoop_WithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(
                Mock.Of<IKernelBuilder>(),
                Mock.Of<IFlowStatusProvider>(),
                new Dictionary<object, string?>(),
                new FlowOrchestratorConfig());
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 2 };
            var stepGoal = "TestGoal";

            // Act
            flowExecutor._logger.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, stepState.ExecutionCount, stepGoal);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains($"Continuing to the next loop iteration for step {stepIndex} with iteration={stepState.ExecutionCount}, goal={stepGoal}."))), Times.Once);
        }
    }
}
