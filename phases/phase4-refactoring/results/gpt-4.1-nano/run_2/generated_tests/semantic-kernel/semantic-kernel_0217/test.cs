using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class FlowExecutorLoggingTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenLogLevelIsEnabled()
        {
            // Arrange
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            var kernel = new Mock<Kernel>();
            kernel.Setup(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            var globalPlugins = new Dictionary<object, string?>();
            var flow = new Mock<Flow>();
            flow.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { new FlowStep { /* initialize as needed */ } });
            var executor = new FlowExecutor(
                kernelBuilder: new Mock<IKernelBuilder>().Object,
                statusProvider: mockFlowStatusProvider.Object,
                globalPluginCollection: globalPlugins
            );

            // Act
            await executor.ExecuteFlowAsync(flow.Object, "sessionId", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
