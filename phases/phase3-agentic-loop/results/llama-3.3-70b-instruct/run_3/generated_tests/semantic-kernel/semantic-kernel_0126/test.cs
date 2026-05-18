using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public void LogDebug_ToolRequests_CallsLoggerLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { ToolCallCount = 1 };

            // Act
            mistralClient._logger = loggerMock.Object;
            mistralClient._logger.IsEnabled(LogLevel.Debug) = true;
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount), Times.Once);
        }

        [Fact]
        public void LogDebug_ToolRequests_DoesNotCallLoggerLogDebug_WhenLogLevelIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { ToolCallCount = 1 };

            // Act
            mistralClient._logger = loggerMock.Object;
            mistralClient._logger.IsEnabled(LogLevel.Debug) = false;
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount), Times.Never);
        }
    }
}
