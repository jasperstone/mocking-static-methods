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
        public async Task ExecuteFlowAsync_LogsInformationOnCompletedStep()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);

            var mockKernel = new Mock<Kernel>(MockBehavior.Strict, null, null, null);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);

            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPluginCollection);

            // Setup a flow with one step that will be completed immediately
            var flow = new Flow("TestFlow");
            var step = new FlowStep("step1", "goal1");
            step.Provides.Add("output1");
            flow.AddStep(step);

            // Setup execution state to simulate completed step
            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables["output1"] = "value1";
            executionState.StepStates[$"0_goal1"] = new ExecutionState.StepExecutionState
            {
                ExecutionCount = 0,
                Status = ExecutionState.Status.Completed
            };

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

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
