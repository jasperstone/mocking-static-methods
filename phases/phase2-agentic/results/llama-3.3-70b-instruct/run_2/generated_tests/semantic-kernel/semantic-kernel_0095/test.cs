using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
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
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    10,
                    20,
                    30),
                Times.Once);
        }

        [Fact]
        public void LogUsage_DoesNotLogInformation_WhenMetadataIsNotAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = null
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public void LogUsage_DoesNotLogInformation_WhenTotalTokenCountIsLessThanOrEqualToZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
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
                        TotalTokenCount = 0
                    }
                }
            };

            // Act
            client.LogUsage(chatMessageContents);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public void ValidateGeminiResponse_ThrowsKernelException_WhenPromptFeedbackBlockReasonIsNotNull()
        {
            // Arrange
            var client = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                null);

            var geminiResponse = new GeminiResponse
            {
                PromptFeedback = new PromptFeedbackElement
                {
                    BlockReason = "block reason"
                }
            };

            // Act and Assert
            Assert.Throws<KernelException>(() => client.ValidateGeminiResponse(geminiResponse));
        }
    }
}
