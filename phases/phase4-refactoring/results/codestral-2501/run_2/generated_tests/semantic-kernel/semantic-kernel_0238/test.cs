using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebug_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            var arguments = new KernelArguments();
            var question = "What is the capital of France?";
            var previousSteps = new List<ReActStep>();

            // Act
            var result = await reActEngine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetNextStepAsync_DoesNotLogDebug_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            var arguments = new KernelArguments();
            var question = "What is the capital of France?";
            var previousSteps = new List<ReActStep>();

            // Act
            var result = await reActEngine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}
