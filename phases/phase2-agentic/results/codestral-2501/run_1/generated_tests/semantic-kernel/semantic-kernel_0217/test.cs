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
        public async Task ExecuteFlowAsync_LogsInformation_WhenExitingLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var config = new FlowOrchestratorConfig();

            var flowExecutor = new FlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, config);
            var flow = new Flow();
            var sessionId = "testSessionId";
            var input = "testInput";
            var kernelArguments = new KernelArguments();

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };

            flowStatusProviderMock.Setup(x => x.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

            var step = new FlowStep
            {
                Goal = "testGoal",
                Provides = new List<string> { "testVariable" },
                Passthrough = new List<string>()
            };

            flow.Steps.Add(step);

            var stepState = new ExecutionState.StepExecutionState
            {
                Status = ExecutionState.Status.InProgress,
                ExecutionCount = 1,
                Output = new Dictionary<string, List<string>>()
            };

            executionState.StepStates.Add("0_testGoal", stepState);

            var stepResult = new FunctionResult
            {
                Metadata = new Dictionary<string, object> { { "testVariable", "testValue" } }
            };

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
