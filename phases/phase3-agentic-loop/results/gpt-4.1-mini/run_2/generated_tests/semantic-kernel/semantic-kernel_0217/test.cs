using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformationWhenExitingLoop()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>(MockBehavior.Strict, null, null, null);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            var flowOrchestratorConfig = new FlowOrchestratorConfig();

            var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPluginCollection, flowOrchestratorConfig);

            // Setup a flow with one step that will trigger the exit loop log
            var flow = new Flow("TestFlow");
            var step = new FlowStep("Goal1");
            step.Provides.Add("output1");
            flow.AddStep(step);

            // Setup execution state to simulate exit loop condition
            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables = new Dictionary<string, string>();
            executionState.StepStates = new Dictionary<string, ExecutionState.StepExecutionState>();

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

            // We need to simulate the stepResult with TryGetExitLoopResponse returning true
            // This requires some internal knowledge or mocking of FunctionResult and step execution
            // Since the code is internal and complex, we will simulate the call by invoking ExecuteFlowAsync and verifying the logger call

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "session1", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exiting loop for step")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
