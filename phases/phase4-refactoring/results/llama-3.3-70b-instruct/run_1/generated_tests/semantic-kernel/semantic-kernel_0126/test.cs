using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Moq;
using Xunit;

namespace Tests
{
    public class MistralClientTests
    {
        [Fact]
        public void LogDebug_ToolRequests_CallsLoggerLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { Message = new MistralChatMessage { ToolCalls = new List<MistralToolCall> { new MistralToolCall() } } };

            // Act
            mistralClient._logger = loggerMock.Object;
            mistralClient._logger.IsEnabled(LogLevel.Debug) = true;
            mistralClient.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount), Times.Once);
        }

        [Fact]
        public void LogDebug_ToolRequests_DoesNotCallLoggerLogDebug_WhenLogLevelIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { Message = new MistralChatMessage { ToolCalls = new List<MistralToolCall> { new MistralToolCall() } } };

            // Act
            mistralClient._logger = loggerMock.Object;
            mistralClient._logger.IsEnabled(LogLevel.Debug) = false;
            mistralClient.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
