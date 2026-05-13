using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
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
            var chatChoice = new MistralChatChoice { Message = new MistralChatMessage { ToolCalls = new List<MistralToolCall> { new MistralToolCall() } } };

            // Act
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", 1), Times.Once);
        }

        [Fact]
        public void LogDebug_ToolRequests_Disabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { Message = new MistralChatMessage { ToolCalls = new List<MistralToolCall> { new MistralToolCall() } } };

            // Act
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
