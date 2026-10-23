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
        public void ProcessChatResponse_ShouldLogInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                mockLogger.Object);

            var metadata = new GeminiResponse.UsageMetadataElement
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var geminiResponse = new GeminiResponse
            {
                Candidates = new List<GeminiResponseCandidate>
                {
                    new GeminiResponseCandidate
                    {
                        Content = new GeminiContent
                        {
                            Parts = new List<GeminiContentPart>
                            {
                                new GeminiContentPart
                                {
                                    Text = "Test content",
                                    Thought = false
                                }
                            }
                        },
                        Metadata = metadata
                    }
                }
            };

            // Act
            client.ProcessChatResponse(geminiResponse);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
