using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests.Gemini.Clients;

public class GeminiChatCompletionClientLogUsageTests
{
    [Fact]
    public void LogUsage_LogsInformation_WhenMetadataPresentAndInfoEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var chatMessageContents = new List<GeminiChatMessageContent>
        {
            new GeminiChatMessageContent(
                AuthorRole.Assistant,
                "test content",
                "test-model",
                null)
            {
                Metadata = new GeminiMetadata
                {
                    PromptTokenCount = 100,
                    CandidatesTokenCount = 50,
                    TotalTokenCount = 150
                }
            }
        };

        var client = new TestableGeminiChatCompletionClient(mockLogger.Object);

        // Act
        client.CallLogUsage(chatMessageContents);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Prompt tokens: 100. Completion tokens: 50. Total tokens: 150.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogUsage_SkipsInfoLog_WhenInfoNotEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var chatMessageContents = new List<GeminiChatMessageContent>
        {
            new GeminiChatMessageContent(
                AuthorRole.Assistant,
                "test content",
                "test-model",
                null)
            {
                Metadata = new GeminiMetadata
                {
                    PromptTokenCount = 100,
                    CandidatesTokenCount = 50,
                    TotalTokenCount = 150
                }
            }
        };

        var client = new TestableGeminiChatCompletionClient(mockLogger.Object);

        // Act
        client.CallLogUsage(chatMessageContents);

        // Assert - No Information log call
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<int>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogUsage_LogsDebug_WhenMetadataNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        var chatMessageContents = new List<GeminiChatMessageContent>
        {
            new GeminiChatMessageContent(AuthorRole.Assistant, "test", "test-model", null)
            {
                Metadata = null!
            }
        };

        var client = new TestableGeminiChatCompletionClient(mockLogger.Object);

        // Act
        client.CallLogUsage(chatMessageContents);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Token usage information unavailable."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogUsage_LogsDebug_WhenTotalTokenCountZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        var chatMessageContents = new List<GeminiChatMessageContent>
        {
            new GeminiChatMessageContent(AuthorRole.Assistant, "test", "test-model", null)
            {
                Metadata = new GeminiMetadata
                {
                    PromptTokenCount = 0,
                    CandidatesTokenCount = 0,
                    TotalTokenCount = 0
                }
            }
        };

        var client = new TestableGeminiChatCompletionClient(mockLogger.Object);

        // Act
        client.CallLogUsage(chatMessageContents);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Token usage information unavailable."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestableGeminiChatCompletionClient
    {
        public ILogger Logger { get; }

        public TestableGeminiChatCompletionClient(ILogger logger)
        {
            this.Logger = logger;
        }

        public void CallLogUsage(List<GeminiChatMessageContent> chatMessageContents)
        {
            GeminiMetadata? metadata = chatMessageContents[0].Metadata;

            if (metadata is null || metadata.TotalTokenCount <= 0)
            {
                this.Logger.LogDebug("Token usage information unavailable.");
                return;
            }

            if (this.Logger.IsEnabled(LogLevel.Information))
            {
                this.Logger.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount);
            }
        }
    }

    private class GeminiMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    private class GeminiChatMessageContent
    {
        public AuthorRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public List<object>? FunctionsToolCalls { get; set; }
        public GeminiMetadata? Metadata { get; set; }

        public GeminiChatMessageContent(AuthorRole role, string content, string modelId, List<object>? functionsToolCalls)
        {
            Role = role;
            Content = content;
            ModelId = modelId;
            FunctionsToolCalls = functionsToolCalls;
        }
    }

    private enum AuthorRole
    {
        Assistant
    }
}
