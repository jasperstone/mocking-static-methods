using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public void LogDebug_ToolRequests_Enabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { ToolCallCount = 1 };

            // Act
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount), Times.Once);
        }

        [Fact]
        public void LogDebug_ToolRequests_Disabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { ToolCallCount = 1 };

            // Act
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
