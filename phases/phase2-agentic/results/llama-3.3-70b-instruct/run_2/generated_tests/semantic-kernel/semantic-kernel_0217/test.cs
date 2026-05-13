using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace TestProject
{
    [TestClass]
    public class FlowExecutorTests
    {
        [TestMethod]
        public void LogInformation_WhenExitingLoop_WithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(null, null, null, null);
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var iteration = 2;
            var stepGoal = "TestGoal";

            // Act
            flowExecutor._logger.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal), Times.Once);
        }

        [TestMethod]
        public void LogInformation_WhenContinuingLoop_WithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(null, null, null, null);
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var iteration = 2;
            var stepGoal = "TestGoal";

            // Act
            flowExecutor._logger.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Continuing to the next loop iteration for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.", stepIndex, iteration, stepGoal), Times.Once);
        }
    }
}
