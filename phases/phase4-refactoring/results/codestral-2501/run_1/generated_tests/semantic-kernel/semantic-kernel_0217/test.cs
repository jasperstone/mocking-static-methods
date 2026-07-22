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
        public async Task ExecuteFlowAsync_ShouldLogExitLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var flowOrchestratorConfig = new FlowOrchestratorConfig();

            var flowExecutor = new Mock<FlowExecutor>(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, flowOrchestratorConfig).Object;

            var flow = new Flow("name", "description");
            var sessionId = "sessionId";
            var input = "input";
            var kernelArguments = new KernelArguments();

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };

            flowStatusProviderMock.Setup(x => x.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

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
