using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace Microsoft.SemanticKernel.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_Should_LogInformation_When_LogLevelEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
            ).Verifiable();

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPlugins = new Dictionary<object, string?>();

            // Create a minimal Flow object with at least one step
            var flow = new Flow
            {
                Name = "TestFlow",
                Steps = new List<FlowStep>
                {
                    new FlowStep { Goal = "Goal1" }
                }
            };
            // Mock the SortSteps method to return the steps as-is
            // For simplicity, assume flow.SortSteps() returns flow.Steps
            // or we can mock the method if needed.

            // Mock the flow status provider to return an execution state with current step index 0
            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, string>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            // Instantiate FlowExecutor with the mocked logger
            var flowExecutor = new FlowExecutor(
                kernelBuilder: mockKernelBuilder.Object,
                statusProvider: mockFlowStatusProvider.Object,
                globalPluginCollection: globalPlugins
            );
            // Inject the mock logger into the private field (via reflection or constructor if possible)
            // For simplicity, assume constructor allows passing logger, or set the field directly if accessible.
            // Since constructor does not accept logger, we can set the private field via reflection.
            var loggerField = typeof(FlowExecutor).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(flowExecutor, mockLogger.Object);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "sessionId", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow TestFlow with sessionId=sessionId.")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
