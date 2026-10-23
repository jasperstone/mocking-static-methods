using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_ShouldLogInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                httpClient: null,
                modelId: "test-model",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: mockLogger.Object);

            var metadata = new GeminiResponse.UsageMetadataElement
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
            typeof(GeminiChatCompletionClient)
                .GetMethod("LogUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(client, new object[] { chatMessageContents });

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
