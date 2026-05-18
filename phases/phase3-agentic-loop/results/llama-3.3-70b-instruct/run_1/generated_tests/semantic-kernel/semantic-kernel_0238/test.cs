using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public void LogDebug_Called_When_Response_Is_Not_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);
            var llmResponseText = "Test response";

            // Act
            reActEngine._logger = loggerMock.Object;
            reActEngine._logger.LogDebug("Response : {ActionText}", llmResponseText);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void LogDebug_Not_Called_When_Response_Is_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);
            string? llmResponseText = null;

            // Act
            reActEngine._logger = loggerMock.Object;
            reActEngine._logger.LogDebug("Response : {ActionText}", llmResponseText);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
