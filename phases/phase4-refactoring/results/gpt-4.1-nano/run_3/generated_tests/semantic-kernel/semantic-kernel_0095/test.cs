using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Text;

namespace SemanticKernel.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private class DummyMetadata
        {
            public int PromptTokenCount { get; set; }
            public int CandidatesTokenCount { get; set; }
            public int TotalTokenCount { get; set; }
        }

        private class DummyContent : GeminiContent
        {
            public override List<KernelContent>? Parts { get; set; }
        }

        private class DummyCandidate : GeminiResponseCandidate
        {
            public override GeminiContent? Content { get; set; }
        }

        private class DummyResponse : GeminiResponse
        {
            public override List<GeminiResponseCandidate>? Candidates { get; set; }
            public GeminiMetadata? Metadata { get; set; }
        }

        [Fact]
        public void LogUsage_ShouldLogInformation_WhenMetadataHasTokensAndLogLevelEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "content", modelId: "model", functionsToolCalls: null)
                {
                    Metadata = metadata
                }
            };

            // Act
            var methodInfo = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(client, new object[] { chatMessageContents });

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenMetadataIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            var client = new GeminiChatCompletionClient(
                new System.Net.Http.HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "content", modelId: "model", functionsToolCalls: null)
                {
                    Metadata = null
                }
            };

            // Act
            var methodInfo = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(client, new object[] { chatMessageContents });

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
