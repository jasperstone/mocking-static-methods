using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_LogsUsageInformation_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var geminiChatCompletionClient = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = new GeminiMetadata
                    {
                        PromptTokenCount = 10,
                        CandidatesTokenCount = 20,
                        TotalTokenCount = 30
                    }
                }
            };

            // Act
            geminiChatCompletionClient.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(l => l.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                10,
                20,
                30),
                Times.Once);
        }

        [Fact]
        public void ProcessChatResponse_ProcessesChatResponseAndLogsUsage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var geminiChatCompletionClient = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var geminiResponse = new GeminiResponse
            {
                Candidates = new List<GeminiResponseCandidate>
                {
                    new GeminiResponseCandidate
                    {
                        Content = new GeminiResponseContent
                        {
                            Parts = new List<GeminiResponseContentPart>
                            {
                                new GeminiResponseContentPart
                                {
                                    Text = "text"
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var chatMessageContents = geminiChatCompletionClient.ProcessChatResponse(geminiResponse);

            // Assert
            loggerMock.Verify(l => l.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
                Times.Once);
        }
    }
}
