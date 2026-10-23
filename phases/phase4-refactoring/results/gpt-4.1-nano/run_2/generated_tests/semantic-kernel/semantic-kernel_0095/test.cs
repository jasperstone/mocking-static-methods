using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_Should_Call_LogInformation_When_Metadata_HasTokens()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new TestGeminiChatCompletionClient(mockLogger.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContent = new GeminiChatMessageContent
            {
                Metadata = metadata
            };

            var chatMessages = new List<GeminiChatMessageContent> { chatMessageContent };

            // Act
            client.InvokeLogUsage(chatMessages);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // A test subclass to access the protected LogUsage method
    internal class TestGeminiChatCompletionClient : GeminiChatCompletionClient
    {
        public TestGeminiChatCompletionClient(ILogger logger) : base(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, logger)
        {
        }

        public void InvokeLogUsage(List<GeminiChatMessageContent> chatMessages)
        {
            this.LogUsage(chatMessages);
        }
    }

    // Dummy classes to compile the test
    public class GeminiMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    public class GeminiChatMessageContent
    {
        public GeminiMetadata Metadata { get; set; }
    }

    public enum GoogleAIVersion
    {
        V1
    }
}
