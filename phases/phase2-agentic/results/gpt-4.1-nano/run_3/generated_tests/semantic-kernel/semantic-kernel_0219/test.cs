using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenLogLevelIsEnabled()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockFlow = new Mock<Flow>();
            var mockKernelArguments = new KernelArguments();

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockKernel.Setup(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var globalPlugins = new Dictionary<object, string?>();
            var executor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            mockFlow.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { new FlowStep { /* initialize as needed */ } });
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(new ExecutionState());

            // Act
            await executor.ExecuteFlowAsync(mockFlow.Object, "sessionId", "input", mockKernelArguments);

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteFlowAsync_ShouldCallLogInformationForCompletion()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockFlow = new Mock<Flow>();
            var mockKernelArguments = new KernelArguments();

            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            mockKernel.Setup(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var globalPlugins = new Dictionary<object, string?>();
            var executor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPlugins);

            mockFlow.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { new FlowStep { /* initialize as needed */ } });
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(new ExecutionState());

            // Act
            await executor.ExecuteFlowAsync(mockFlow.Object, "sessionId", "input", mockKernelArguments);

            // Assert
            mockLogger.Verify(l => l.LogInformation("Executing flow {FlowName} with sessionId={SessionId}.", It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
