using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Flow.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task LogInformation_ShouldBeCalled_WhenStepIsCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var config = new FlowOrchestratorConfig();

            var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, config);

            var flow = new Flow
            {
                Name = "TestFlow",
                Steps = new List<FlowStep>
                {
                    new FlowStep
                    {
                        Goal = "TestGoal",
                        Provides = new List<string> { "TestVariable" },
                        Passthrough = new List<string> { "TestVariable" },
                        CompletionType = CompletionType.AtLeastOnce
                    }
                }
            };

            var sessionId = "TestSessionId";
            var input = "TestInput";
            var kernelArguments = new KernelArguments();

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string> { { "TestVariable", "TestValue" } },
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>
                {
                    { "0_TestGoal", new ExecutionState.StepExecutionState { ExecutionCount = 0, Status = ExecutionState.Status.Completed } }
                }
            };

            flowStatusProviderMock.Setup(x => x.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
