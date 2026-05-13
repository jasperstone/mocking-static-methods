using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private class TestGeminiChatCompletionClient : GeminiChatCompletionClient
        {
            public TestGeminiChatCompletionClient(ILogger logger)
                : base(
                    httpClient: new System.Net.Http.HttpClient(),
                    modelId: "test-model",
                    apiKey: "test-api-key",
                    apiVersion: GoogleAIVersion.V1,
                    logger: logger)
            {
            }

            public void CallLogUsage(List<GeminiChatMessageContent> chatMessageContents)
            {
                // Call the private LogUsage method via reflection
                var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(this, new object[] { chatMessageContents });
            }
        }

        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataHasValidTokenCounts()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new TestGeminiChatCompletionClient(loggerMock.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContent = new GeminiChatMessageContent(
                role: AuthorRole.Assistant,
                content: "test",
                modelId: "test-model",
                functionsToolCalls: null)
            {
                Metadata = metadata
            };

            var chatMessageContents = new List<GeminiChatMessageContent> { chatMessageContent };

            // Act
            client.CallLogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new TestGeminiChatCompletionClient(loggerMock.Object);

            var chatMessageContent = new GeminiChatMessageContent(
                role: AuthorRole.Assistant,
                content: "test",
                modelId: "test-model",
                functionsToolCalls: null)
            {
                Metadata = null
            };

            var chatMessageContents = new List<GeminiChatMessageContent> { chatMessageContent };

            // Act
            client.CallLogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Token usage information unavailable.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenTotalTokenCountIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new TestGeminiChatCompletionClient(loggerMock.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 0,
                CandidatesTokenCount = 0,
                TotalTokenCount = 0
            };

            var chatMessageContent = new GeminiChatMessageContent(
                role: AuthorRole.Assistant,
                content: "test",
                modelId: "test-model",
                functionsToolCalls: null)
            {
                Metadata = metadata
            };

            var chatMessageContents = new List<GeminiChatMessageContent> { chatMessageContent };

            // Act
            client.CallLogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Token usage information unavailable.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
