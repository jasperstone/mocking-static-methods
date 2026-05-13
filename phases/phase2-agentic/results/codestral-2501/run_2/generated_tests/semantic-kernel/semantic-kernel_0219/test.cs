using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation_WhenStepIsCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var flowOrchestratorConfig = new FlowOrchestratorConfig();

            var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, flowOrchestratorConfig);

            var flow = new Flow
            {
                Name = "TestFlow",
                Steps = new List<FlowStep>
                {
                    new FlowStep
                    {
                        Goal = "TestGoal",
                        Provides = new List<string> { "TestVariable" },
                        Passthrough = new List<string> { "TestVariable" }
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
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step 0 for iteration=0, goal=TestGoal.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
