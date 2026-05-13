using Moq;
using Microsoft.Extensions.Logging;
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

            var chatMessageContent = new GeminiChatMessageContent
            {
                Metadata = metadata
            };

            var chatMessageContents = new List<GeminiChatMessageContent> { chatMessageContent };

            mockLogger
                .Setup(l => l.IsEnabled(LogLevel.Information))
                .Returns(true);

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.")),
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount),
                Times.Once);
        }
    }
}
