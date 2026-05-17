using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Connectors.Google.Core
{
    internal class GeminiMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    internal class GeminiChatMessageContent
    {
        public GeminiMetadata? Metadata { get; set; }
    }

    public class GeminiChatCompletionClientTests
    {
        private static void InvokeLogUsage(GeminiChatCompletionClient client, List<GeminiChatMessageContent> contents)
        {
            MethodInfo? logUsageMethod = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(logUsageMethod);
            logUsageMethod.Invoke(client, new object[] { contents });
        }

        [Fact]
        public void LogUsage_LogsInformation_WhenMetadataIsValidAndLogLevelInformationEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var contents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = new GeminiMetadata
                    {
                        PromptTokenCount = 5,
                        CandidatesTokenCount = 10,
                        TotalTokenCount = 15
                    }
                }
            };

            // Act
            InvokeLogUsage(client, contents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Prompt tokens: 5. Completion tokens: 10. Total tokens: 15.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var contents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = null
                }
            };

            // Act
            InvokeLogUsage(client, contents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token usage information unavailable.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_LogsDebug_WhenTotalTokenCountIsZeroOrLess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var contents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent
                {
                    Metadata = new GeminiMetadata
                    {
                        PromptTokenCount = 5,
                        CandidatesTokenCount = 10,
                        TotalTokenCount = 0
                    }
                }
            };

            // Act
            InvokeLogUsage(client, contents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token usage information unavailable.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
