using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Experimental.Orchestration.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration;

namespace Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebug_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Debug)).Returns(true);

            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            await reActEngine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
