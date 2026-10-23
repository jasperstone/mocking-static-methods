using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenLoggingEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>();
            var loggerFactory = new LoggerFactory();
            mockKernel.Setup(k => k.LoggerFactory).Returns(loggerFactory);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var mockStatusProvider = new Mock<IFlowStatusProvider>();
            var executionState = new ExecutionState
            {
                CurrentStepIndex = 0,
                Variables = new Dictionary<string, object>(),
                StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
            };
            mockStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(executionState);

            var globalPlugins = new Dictionary<object, string?>();
            var flow = new Flow { Name = "TestFlow" };
            flow.Steps = new List<FlowStep> { new FlowStep { Goal = "Goal1", Provides = new List<string>() } };
            // Assuming SortSteps is an extension method or property, mock or set accordingly
            // For simplicity, assume flow.Steps is already sorted

            var executor = new FlowExecutor(
                kernelBuilder: mockKernelBuilder.Object,
                statusProvider: mockStatusProvider.Object,
                globalPluginCollection: globalPlugins
            );

            // Act
            await executor.ExecuteFlowAsync(flow, "sessionId", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }
    }
}
