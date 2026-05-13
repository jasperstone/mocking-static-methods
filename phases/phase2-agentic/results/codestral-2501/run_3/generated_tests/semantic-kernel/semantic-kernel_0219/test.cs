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
        public async Task ExecuteFlowAsync_LogsInformation_WhenStepIsCompleted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>();
            var mockReActEngine = new Mock<ReActEngine>();

            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);
            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);

            var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, new Dictionary<object, string?>());

            var flow = new Flow
            {
                Name = "TestFlow",
                Steps = new List<FlowStep>
                {
                    new FlowStep
                    {
                        Goal = "TestGoal",
                        Provides = new List<string> { "TestVariable" },
                        Passthrough = new List<string> { "TestVariable" }
                    }
                }
            };

            var executionState = new ExecutionState
            {
                Variables = new Dictionary<string, string> { { "TestVariable", "TestValue" } },
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>
                {
                    { "0_TestGoal", new ExecutionState.StepExecutionState { ExecutionCount = 0, Status = ExecutionState.Status.Completed } }
                }
            };

            mockFlowStatusProvider.Setup(fsp => fsp.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, "TestSessionId", "TestInput", new KernelArguments());

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step 0 for iteration=0, goal=TestGoal.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
