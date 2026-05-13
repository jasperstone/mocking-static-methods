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
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            var flowExecutor = new TestableFlowExecutor(kernelBuilderMock.Object, flowStatusProviderMock.Object, globalPluginCollection, loggerMock.Object);

            var flow = new Flow("TestFlow");
            var step = new FlowStep("Goal1");
            step.Provides.Add("output1");
            flow.AddStep(step);

            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables = new Dictionary<string, string>();
            executionState.StepStates = new Dictionary<string, ExecutionState.StepExecutionState>();

            flowStatusProviderMock.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

            var kernelArguments = new KernelArguments();

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "session1", "input", kernelArguments);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exiting loop for step")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        private class TestableFlowExecutor : FlowExecutor
        {
            public TestableFlowExecutor(IKernelBuilder kernelBuilder, IFlowStatusProvider flowStatusProvider, Dictionary<object, string?> globalPluginCollection, ILogger logger)
                : base(kernelBuilder, flowStatusProvider, globalPluginCollection)
            {
                this.OverrideLogger(logger);
            }

            public void OverrideLogger(ILogger logger)
            {
                var loggerField = typeof(FlowExecutor).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                loggerField!.SetValue(this, logger);
            }
        }
    }
}
