using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_Should_LogInformation_When_LoggerEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            mockLogger.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
            ).Verifiable();

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>();
            mockKernel.Setup(k => k.LoggerFactory).Returns(Mock.Of<ILoggerFactory>());
            mockKernel.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, object>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            var globalPlugins = new Dictionary<object, string?>();
            var executor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            // Create a dummy flow with one step
            var flow = new Flow
            {
                Name = "TestFlow",
                Steps = new List<FlowStep>
                {
                    new FlowStep
                    {
                        Goal = "TestGoal",
                        Provides = new List<string> { "var1" }
                    }
                }
            };
            flow.SortSteps = () => flow.Steps;

            // Setup the flow to simulate completion
            executionState.CurrentStepIndex = 0;
            executionState.Variables["var1"] = "value";

            // Act
            await executor.ExecuteFlowAsync(flow, "sessionId", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step 0 for iteration=0, goal=TestGoal.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
