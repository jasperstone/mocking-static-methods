using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;

namespace SemanticKernel.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private class DummyChatHistory : ChatHistory
        {
            public override IReadOnlyList<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        }

        [Fact]
        public void LogUsage_ShouldLogInformation_WhenMetadataHasTokensAndLoggerEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var client = new DummyGeminiClient(mockLogger.Object);
            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };
            var chatContent = new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "test", modelId: "model", functionsToolCalls: null)
            {
                Metadata = metadata
            };
            var chatContents = new List<GeminiChatMessageContent> { chatContent };

            // Act
            client.InvokeLogUsage(chatContents);

            // Assert
            mockLogger.Verify(x => x.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                metadata.PromptTokenCount,
                metadata.CandidatesTokenCount,
                metadata.TotalTokenCount), Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLog_WhenMetadataIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new DummyGeminiClient(mockLogger.Object);
            var chatContent = new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "test", modelId: "model", functionsToolCalls: null)
            {
                Metadata = null
            };
            var chatContents = new List<GeminiChatMessageContent> { chatContent };

            // Act
            client.InvokeLogUsage(chatContents);

            // Assert
            mockLogger.Verify(x => x.LogDebug("Token usage information unavailable."), Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLog_WhenMetadataTotalTokenCountIsZeroOrLess()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var client = new DummyGeminiClient(mockLogger.Object);
            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 0
            };
            var chatContent = new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "test", modelId: "model", functionsToolCalls: null)
            {
                Metadata = metadata
            };
            var chatContents = new List<GeminiChatMessageContent> { chatContent };

            // Act
            client.InvokeLogUsage(chatContents);

            // Assert
            mockLogger.Verify(x => x.LogDebug("Token usage information unavailable."), Times.Once);
        }

        // Helper class to test LogUsage method
        private class DummyGeminiClient : GeminiChatCompletionClient
        {
            public DummyGeminiClient(ILogger logger) : base(new HttpClient(), "model", "apiKey", GoogleAIVersion.V1, logger)
            {
            }

            public void InvokeLogUsage(List<GeminiChatMessageContent> chatMessageContents)
            {
                // Call the protected method
                this.LogUsage(chatMessageContents);
            }
        }
    }
}
