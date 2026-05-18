using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenLoggerIsNotEnabledForInformation()
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
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
