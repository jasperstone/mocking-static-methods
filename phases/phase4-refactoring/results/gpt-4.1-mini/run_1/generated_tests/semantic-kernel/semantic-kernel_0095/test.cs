using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private class TestGeminiChatMessageContent
        {
            public TestGeminiMetadata? Metadata { get; set; }
        }

        private class TestGeminiMetadata
        {
            public int PromptTokenCount { get; }
            public int CandidatesTokenCount { get; }
            public int TotalTokenCount { get; }

            public TestGeminiMetadata(int promptTokens, int candidatesTokens, int totalTokens)
            {
                PromptTokenCount = promptTokens;
                CandidatesTokenCount = candidatesTokens;
                TotalTokenCount = totalTokens;
            }
        }

        // Public subclass to expose LogUsage for testing
        public class TestableGeminiChatCompletionClient : GeminiChatCompletionClient
        {
            public TestableGeminiChatCompletionClient(ILogger? logger)
                : base(
                    httpClient: null!,
                    modelId: "test-model",
                    apiKey: "test-key",
                    apiVersion: GoogleAIVersion.V1,
                    logger: logger)
            {
            }

            public void CallLogUsage(List<object> chatMessageContents)
            {
                // Call the private method via reflection
                var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("LogUsage method not found");
                method.Invoke(this, new object[] { chatMessageContents });
            }
        }

        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataHasTokenCounts()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new TestableGeminiChatCompletionClient(loggerMock.Object);

            var metadata = new TestGeminiMetadata(10, 20, 30);
            var chatMessageContent = new TestGeminiChatMessageContent { Metadata = metadata };
            var chatMessageContents = new List<object> { chatMessageContent };

            // Act
            client.CallLogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new TestableGeminiChatCompletionClient(loggerMock.Object);

            var chatMessageContent = new TestGeminiChatMessageContent { Metadata = null };
            var chatMessageContents = new List<object> { chatMessageContent };

            // Act
            client.CallLogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token usage information unavailable.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenTotalTokenCountIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new TestableGeminiChatCompletionClient(loggerMock.Object);

            var metadata = new TestGeminiMetadata(0, 0, 0);
            var chatMessageContent = new TestGeminiChatMessageContent { Metadata = metadata };
            var chatMessageContents = new List<object> { chatMessageContent };

            // Act
            client.CallLogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token usage information unavailable.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
