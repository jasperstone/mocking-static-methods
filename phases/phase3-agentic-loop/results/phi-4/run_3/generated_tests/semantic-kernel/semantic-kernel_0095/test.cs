using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_ShouldLogInformation_WhenLoggerIsEnabledForInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "model-id",
                "api-key",
                GoogleAIVersion.V1,
                mockLogger.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(metadata)
            };

            mockLogger
                .Setup(l => l.IsEnabled(LogLevel.Information))
                .Returns(true);

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount),
                Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenLoggerIsNotEnabledForInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "model-id",
                "api-key",
                GoogleAIVersion.V1,
                mockLogger.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(metadata)
            };

            mockLogger
                .Setup(l => l.IsEnabled(LogLevel.Information))
                .Returns(false);

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }
    }
}
