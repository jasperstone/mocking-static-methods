using System;
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
        public void LogUsage_LogsInformation_WhenMetadataIsAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                httpClient: new System.Net.Http.HttpClient(),
                modelId: "test-model",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: loggerMock.Object);

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
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                httpClient: new System.Net.Http.HttpClient(),
                modelId: "test-model",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: loggerMock.Object);

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
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenTotalTokenCountIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
            var client = new GeminiChatCompletionClient(
                httpClient: new System.Net.Http.HttpClient(),
                modelId: "test-model",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: loggerMock.Object);

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
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
