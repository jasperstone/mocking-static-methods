using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Flow.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation_WhenStepIsCompleted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();
            var config = new FlowOrchestratorConfig();

            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };

            flowStatusProviderMock.Setup(x => x.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernelBuilderMock.Setup(x => x.Build()).Returns(kernelMock.Object);

            var flowExecutor = new FlowExecutor(
                kernelBuilderMock.Object,
                flowStatusProviderMock.Object,
                globalPluginCollection,
                config);

            var flow = new Flow
            {
                Steps = new List<FlowStep>
                {
                    new FlowStep("TestGoal")
                }
            };

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "sessionId", "input", new KernelArguments());

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
