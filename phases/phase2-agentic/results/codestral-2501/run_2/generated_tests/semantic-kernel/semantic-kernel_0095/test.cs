using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

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
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogUsage_DoesNotLogInformation_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

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
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        [Fact]
        public void LogUsage_DoesNotLogInformation_WhenTotalTokenCountIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

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
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}
