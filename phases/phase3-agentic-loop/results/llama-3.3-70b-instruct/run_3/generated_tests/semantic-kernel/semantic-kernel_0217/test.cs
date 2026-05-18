using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task LogInformation_WhenExitingLoop_WithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var flowOrchestratorConfig = new FlowOrchestratorConfig();

            var flowExecutor = new FlowExecutor(
                kernelBuilderMock.Object,
                flowStatusProviderMock.Object,
                globalPluginCollection,
                flowOrchestratorConfig
            );
            flowExecutor._logger = loggerMock.Object;

            var flow = new Flow();
            var step = new FlowStep();
            step.Goal = "TestGoal";
            flow.Steps.Add(step);

            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.StepStates.Add("0_TestGoal", new ExecutionState.StepExecutionState());

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "sessionId", "input", new KernelArguments());

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Exiting loop for step {StepIndex} with iteration={Iteration}, goal={StepGoal}.",
                    0,
                    1,
                    "TestGoal"
                ),
                Times.Once
            );
        }
    }
}
