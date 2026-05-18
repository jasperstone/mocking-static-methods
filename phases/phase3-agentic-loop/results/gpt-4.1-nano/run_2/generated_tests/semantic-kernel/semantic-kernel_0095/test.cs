using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public GeminiChatCompletionClientTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LogUsage_ShouldLogInformation_WhenMetadataHasTokensAndLogLevelEnabled()
        {
            // Arrange
            var client = new TestGeminiChatCompletionClient(_loggerMock.Object);
            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };
            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "test", modelId: "model", functionsToolCalls: null)
                {
                    Metadata = metadata
                }
            };

            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            client.InvokePrivateLogUsage(chatMessageContents);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenMetadataHasNoTokens()
        {
            // Arrange
            var client = new TestGeminiChatCompletionClient(_loggerMock.Object);
            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "test", modelId: "model", functionsToolCalls: null)
                {
                    Metadata = null
                }
            };

            // Act
            client.InvokePrivateLogUsage(chatMessageContents);

            // Assert
            _loggerMock.Verify(x => x.LogDebug("Token usage information unavailable."), Times.Once);
        }

        [Fact]
        public void LogUsage_ShouldNotLogInformation_WhenLogLevelDisabled()
        {
            // Arrange
            var client = new TestGeminiChatCompletionClient(_loggerMock.Object);
            var metadata = new GeminiMetadata
            {
                PromptTokenCount = 10,
                CandidatesTokenCount = 20,
                TotalTokenCount = 30
            };
            var chatMessageContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(role: AuthorRole.Assistant, content: "test", modelId: "model", functionsToolCalls: null)
                {
                    Metadata = metadata
                }
            };

            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(false);

            // Act
            client.InvokePrivateLogUsage(chatMessageContents);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>()), Times.Never);
        }

        // Helper class to access private method
        private class TestGeminiChatCompletionClient : GeminiChatCompletionClient
        {
            public TestGeminiChatCompletionClient(ILogger logger) : base(
                httpClient: new System.Net.Http.HttpClient(),
                modelId: "model",
                apiKey: "apiKey",
                apiVersion: GoogleAIVersion.V1,
                logger: logger)
            {
            }

            public void InvokePrivateLogUsage(List<GeminiChatMessageContent> chatMessageContents)
            {
                // Call the private method via reflection
                var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(this, new object[] { chatMessageContents });
            }
        }
    }

    // Dummy classes to compile the test
    public class GeminiMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    public class GeminiChatMessageContent
    {
        public AuthorRole Role { get; }
        public string Content { get; }
        public string ModelId { get; }
        public object? FunctionsToolCalls { get; }
        public GeminiMetadata? Metadata { get; set; }

        public GeminiChatMessageContent(AuthorRole role, string content, string modelId, object? functionsToolCalls)
        {
            Role = role;
            Content = content;
            ModelId = modelId;
            FunctionsToolCalls = functionsToolCalls;
        }
    }

    public enum AuthorRole
    {
        User,
        Assistant
    }

    public enum GoogleAIVersion
    {
        V1
    }
}
