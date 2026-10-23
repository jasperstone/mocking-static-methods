using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SemanticKernel.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task LogUsage_Should_Call_LogInformation_When_Metadata_HasTokensAndLogLevelEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var client = new DummyGeminiClient(mockLogger.Object);
            var chatContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(
                    role: AuthorRole.Assistant,
                    content: "test",
                    modelId: "model",
                    functionsToolCalls: null)
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
            client.InvokeLogUsage(chatContents);

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Prompt tokens: 10. Completion tokens: 20. Total tokens: 30.")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public void LogUsage_Should_Call_LogDebug_When_Metadata_Is_Null()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            var client = new DummyGeminiClient(mockLogger.Object);
            var chatContents = new List<GeminiChatMessageContent>
            {
                new GeminiChatMessageContent(
                    role: AuthorRole.Assistant,
                    content: "test",
                    modelId: "model",
                    functionsToolCalls: null)
                {
                    Metadata = null
                }
            };

            // Act
            client.InvokeLogUsage(chatContents);

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Token usage information unavailable.")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }

    // Dummy class to access the LogUsage method
    internal class DummyGeminiClient : GeminiChatCompletionClient
    {
        public DummyGeminiClient(ILogger logger) : base(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, logger)
        {
        }

        public void InvokeLogUsage(List<GeminiChatMessageContent> chatContents)
        {
            this.LogUsage(chatContents);
        }
    }

    // Dummy enum for GoogleAIVersion
    public enum GoogleAIVersion
    {
        V1
    }

    // Dummy classes for GeminiMetadata and GeminiChatMessageContent
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
}
