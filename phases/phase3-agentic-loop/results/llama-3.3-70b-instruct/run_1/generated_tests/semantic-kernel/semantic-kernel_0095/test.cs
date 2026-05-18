using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Connectors.Google.Core;

namespace Microsoft.SemanticKernel.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);
            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = new GeminiMetadata
                    {
                        PromptTokenCount = 10,
                        CandidatesTokenCount = 20,
                        TotalTokenCount = 30
                    }
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenMetadataIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);
            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = null
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogUsage_DoesNotLogInformation_WhenLoggerIsNotEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);
            var client = new GeminiChatCompletionClient(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, loggerMock.Object);
            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = new GeminiMetadata
                    {
                        PromptTokenCount = 10,
                        CandidatesTokenCount = 20,
                        TotalTokenCount = 30
                    }
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
