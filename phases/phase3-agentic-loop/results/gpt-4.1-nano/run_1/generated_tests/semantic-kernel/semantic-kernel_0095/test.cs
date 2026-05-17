using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Connectors.Google.Core;

namespace GeminiClientTests
{
    public class GeminiChatCompletionClientTests
    {
        private class DummyContent
        {
            public List<KernelContentPart> Parts { get; set; }
        }

        private class KernelContentPart
        {
            public bool? Thought { get; set; }
            public string Text { get; set; }
        }

        private class DummyMetadata
        {
            public int PromptTokenCount { get; set; }
            public int CandidatesTokenCount { get; set; }
            public int TotalTokenCount { get; set; }
        }

        private class DummyChatMessageContent
        {
            public DummyMetadata Metadata { get; set; }
        }

        [Fact]
        public void LogUsage_ShouldLogInformation_WhenMetadataIsValidAndLoggerEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var client = new GeminiChatCompletionClientStub(mockLogger.Object);

            var metadata = new DummyMetadata
            {
                PromptTokenCount = 1,
                CandidatesTokenCount = 2,
                TotalTokenCount = 3
            };

            var chatMessages = new List<DummyChatMessageContent>
            {
                new DummyChatMessageContent { Metadata = metadata }
            };

            // Act
            client.CallLogUsage(chatMessages);

            // Assert
            mockLogger.Verify(x => x.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                metadata.PromptTokenCount,
                metadata.CandidatesTokenCount,
                metadata.TotalTokenCount), Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenMetadataIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var client = new GeminiChatCompletionClientStub(mockLogger.Object);

            var chatMessages = new List<DummyChatMessageContent>
            {
                new DummyChatMessageContent { Metadata = null }
            };

            // Act
            client.CallLogUsage(chatMessages);

            // Assert
            mockLogger.Verify(x => x.LogDebug("Token usage information unavailable."), Times.Once);
            mockLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenTotalTokenCountIsZeroOrLess()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var client = new GeminiChatCompletionClientStub(mockLogger.Object);

            var metadata = new DummyMetadata
            {
                PromptTokenCount = 1,
                CandidatesTokenCount = 2,
                TotalTokenCount = 0
            };

            var chatMessages = new List<DummyChatMessageContent>
            {
                new DummyChatMessageContent { Metadata = metadata }
            };

            // Act
            client.CallLogUsage(chatMessages);

            // Assert
            mockLogger.Verify(x => x.LogDebug("Token usage information unavailable."), Times.Once);
            mockLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        // Helper class to access the protected method
        private class GeminiChatCompletionClientStub : GeminiChatCompletionClient
        {
            public GeminiChatCompletionClientStub(ILogger logger) : base(new System.Net.Http.HttpClient(), "model", "apiKey", GoogleAIVersion.V1, logger)
            {
            }

            public void CallLogUsage(List<DummyChatMessageContent> chatMessages)
            {
                // Call the protected method
                this.LogUsage(chatMessages.Select(c => new GeminiChatMessageContent
                {
                    Metadata = c.Metadata
                }).ToList());
            }
        }
    }
}
