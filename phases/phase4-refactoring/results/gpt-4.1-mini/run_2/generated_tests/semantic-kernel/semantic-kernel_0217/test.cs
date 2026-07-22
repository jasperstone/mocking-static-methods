using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class FlowOrchestratorLoggerTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformationOnExitingLoop()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>(MockBehavior.Loose, null, null, null);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(object))).Returns(mockLogger.Object);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            var flowOrchestrator = new FlowOrchestrator(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPluginCollection);

            // Setup a flow with one step that triggers the exit loop logging
            var flow = new Flow("TestFlow", null);
            var step = new FlowStep("StepGoal");
            step.AddProvides("output1");
            flow.AddStep(step);

            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables["output1"] = "value";

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

            var kernelArgs = new KernelArguments();

            // Act
            await flowOrchestrator.ExecuteFlowAsync(flow, "session1", "input", kernelArgs);

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
