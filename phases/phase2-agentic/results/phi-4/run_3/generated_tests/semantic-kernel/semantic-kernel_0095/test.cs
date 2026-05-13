using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_ShouldLogInformation_WhenLogLevelIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                httpClient: null, // Assuming HttpClient is not needed for this test
                modelId: "test-model-id",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: mockLogger.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = metadata
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

            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount),
                Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenLogLevelIsNotEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Information)).Returns(false);

            var client = new GeminiChatCompletionClient(
                httpClient: null, // Assuming HttpClient is not needed for this test
                modelId: "test-model-id",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: mockLogger.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = metadata
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount),
                Times.Never);
        }
    }
}
