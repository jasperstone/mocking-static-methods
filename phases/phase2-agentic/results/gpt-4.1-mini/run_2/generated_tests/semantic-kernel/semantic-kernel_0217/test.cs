using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation_WhenExitLoop()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>(null, null, null);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPlugins = new Dictionary<object, string?>();

            var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            var flow = new Flow("TestFlow");
            var step = new FlowStep("Step1", "Goal1");
            step.Provides.Add("output1");
            flow.AddStep(step);

            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables = new Dictionary<string, string>();

            var stepKey = $"0_{step.Goal1}";
            var stepState = new ExecutionState.StepExecutionState();
            executionState.StepStates[stepKey] = stepState;

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

            var kernelArgs = new KernelArguments();

            // Act
            // We cannot directly invoke the private method or the internal loop that triggers the log.
            // So we simulate the condition by calling ExecuteFlowAsync and rely on the flow and mocks.
            // This test mainly ensures the logger is called with the expected message.
            await flowExecutor.ExecuteFlowAsync(flow, "session1", "input", kernelArgs);

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
