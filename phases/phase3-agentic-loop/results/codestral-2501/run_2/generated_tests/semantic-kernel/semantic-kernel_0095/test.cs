using Xunit;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(
                    role: AuthorRole.Assistant,
                    content: "Test content",
                    modelId: "modelId",
                    functionsToolCalls: null,
                    metadata: new GeminiMetadata
                    {
                        PromptTokenCount = 10,
                        CandidatesTokenCount = 5,
                        TotalTokenCount = 15
                    })
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(
                    role: AuthorRole.Assistant,
                    content: "Test content",
                    modelId: "modelId",
                    functionsToolCalls: null,
                    metadata: null)
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
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
