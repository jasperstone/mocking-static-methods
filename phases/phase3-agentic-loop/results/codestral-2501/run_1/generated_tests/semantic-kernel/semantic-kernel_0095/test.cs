using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_ShouldLogInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                mockLogger.Object);

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
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldLogDebug_WhenMetadataIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                mockLogger.Object);

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
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldLogDebug_WhenTotalTokenCountIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                mockLogger.Object);

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = new GeminiMetadata
                    {
                        PromptTokenCount = 10,
                        CandidatesTokenCount = 20,
                        TotalTokenCount = 0
                    }
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
