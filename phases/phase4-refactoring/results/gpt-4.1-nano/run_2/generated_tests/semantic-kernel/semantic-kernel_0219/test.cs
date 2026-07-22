using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;

namespace Microsoft.SemanticKernel.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenLoggingEnabled()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<IKernel>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPlugins = new Dictionary<object, string?>();

            // Setup logger factory to return our mock logger
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockKernel.Setup(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            // Setup flow and flow steps
            var flow = new Flow { Name = "TestFlow" };
            flow.AddStep(new FlowStep { Goal = "Goal1", Provides = new List<string> { "var1" } });
            flow.SortSteps();

            // Setup flow state
            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, object> { { "var1", "value1" } },
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            var executor = new FlowExecutor(
                kernelBuilder: mockKernelBuilder.Object,
                statusProvider: mockFlowStatusProvider.Object,
                globalPluginCollection: globalPlugins
            );

            // Act
            var result = await executor.ExecuteFlowAsync(flow, "session123", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow TestFlow with sessionId=session123.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
