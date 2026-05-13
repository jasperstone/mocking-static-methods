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
        public void LogInformation_Called_WhenStepIsCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowExecutor = new FlowExecutor(
                Mock.Of<IKernelBuilder>(),
                Mock.Of<IFlowStatusProvider>(),
                new Dictionary<object, string?>(),
                new FlowOrchestratorConfig()
            );
            flowExecutor._logger = loggerMock.Object;

            var stepIndex = 1;
            var stepState = new ExecutionState.StepExecutionState { ExecutionCount = 1 };
            var step = new FlowStep { Goal = "TestGoal" };

            // Act
            flowExecutor.ExecuteFlowAsync(
                new Flow { Steps = new[] { step } },
                "sessionId",
                "input",
                new KernelArguments()
            ).Wait();

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
                    stepIndex,
                    stepState.ExecutionCount,
                    step.Goal
                ),
                Times.Once
            );
        }
    }
}
