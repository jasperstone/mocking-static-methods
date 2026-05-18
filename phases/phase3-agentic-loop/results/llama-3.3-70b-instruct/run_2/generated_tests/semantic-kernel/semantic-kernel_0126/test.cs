using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public void LogDebug_ToolRequests_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { Message = new MistralChatMessage { ToolCalls = new List<MistralToolCall> { new MistralToolCall(), new MistralToolCall() } } };

            // Act
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Tool requests: 2"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_ToolRequests_DoesNotLogIfNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            var mistralClient = new MistralClient("modelId", new HttpClient(), "apiKey", logger: loggerMock.Object);
            var chatChoice = new MistralChatChoice { Message = new MistralChatMessage { ToolCalls = new List<MistralToolCall> { new MistralToolCall(), new MistralToolCall() } } };

            // Act
            mistralClient._logger.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
