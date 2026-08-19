using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var geminiChatCompletionClient = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var geminiResponse = new GeminiResponse
            {
                Candidates = new[]
                {
                    new GeminiResponseCandidate
                    {
                        Content = "content",
                        ModelId = "modelId"
                    }
                }
            };

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            // Act
            geminiChatCompletionClient.LogUsage(new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = metadata
                }
            });

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount),
                Times.Once);
        }
    }
}
