using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution
{
    internal class FlowExecutorLoggerExtensionsTests
    {
        private class TestFlowExecutor : FlowExecutor
        {
            public TestFlowExecutor(IKernelBuilder kernelBuilder, IFlowStatusProvider statusProvider, Dictionary<object, string?> globalPluginCollection)
                : base(kernelBuilder, statusProvider, globalPluginCollection)
            {
            }

            public new Task<FunctionResult> ExecuteFlowAsync(Flow flow, string sessionId, string input, KernelArguments kernelArguments)
            {
                return base.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);
            }
        }

        [Fact]
        public async Task ExecuteFlow_LogsInformationOnStepCompletion()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernel = new Mock<Kernel>(MockBehavior.Strict, null, null, null);
            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            var flow = new Flow("TestFlow", "TestGoal");
            var step = new FlowStep("StepGoal");
            flow.AddStep(step);

            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables["output1"] = "value1";
            executionState.StepStates[$"0_{step.Goal}"] = new ExecutionState.StepExecutionState
            {
                ExecutionCount = 1,
                Status = ExecutionState.Status.Completed
            };

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            var flowExecutor = new TestFlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPluginCollection);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "session1", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
