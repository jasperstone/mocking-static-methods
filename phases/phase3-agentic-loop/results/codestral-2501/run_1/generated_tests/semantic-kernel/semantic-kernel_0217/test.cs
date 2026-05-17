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
    public class FlowExecutorWrapper : FlowExecutor
    {
        public FlowExecutorWrapper(IKernelBuilder kernelBuilder, IFlowStatusProvider statusProvider, Dictionary<object, string?> globalPluginCollection, FlowOrchestratorConfig? config = null)
            : base(kernelBuilder, statusProvider, globalPluginCollection, config)
        {
        }

        public new Task<FunctionResult> ExecuteFlowAsync(Flow flow, string sessionId, string input, KernelArguments kernelArguments)
        {
            return base.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);
        }
    }

    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenExitingLoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var kernelMock = new Mock<Kernel>();
            var flowMock = new Mock<Flow>();
            var stepMock = new Mock<FlowStep>();
            var stepResultMock = new Mock<FunctionResult>();
            var executionStateMock = new Mock<ExecutionState>();

            kernelBuilderMock.Setup(kb => kb.Build()).Returns(kernelMock.Object);
            kernelMock.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var flowExecutor = new FlowExecutorWrapper(kernelBuilderMock.Object, flowStatusProviderMock.Object, new Dictionary<object, string?>());

            flowMock.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { stepMock.Object });
            flowStatusProviderMock.Setup(fsp => fsp.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionStateMock.Object);

            stepMock.Setup(s => s.Provides).Returns(new List<string> { "variable1" });
            stepMock.Setup(s => s.Passthrough).Returns(new List<string> { "variable2" });
            stepMock.Setup(s => s.Goal).Returns("goal");

            stepResultMock.Setup(sr => sr.TryGetExitLoopResponse(out It.Ref<string>.IsAny)).Returns(true);
            stepResultMock.Setup(sr => sr.Metadata).Returns(new Dictionary<string, object> { { "variable1", "value1" } });

            executionStateMock.Setup(es => es.Variables).Returns(new Dictionary<string, string> { { "variable1", "value1" } });
            executionStateMock.Setup(es => es.StepStates).Returns(new Dictionary<string, ExecutionState.StepExecutionState>());

            // Act
            await flowExecutor.ExecuteFlowAsync(flowMock.Object, "sessionId", "input", new KernelArguments());

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
