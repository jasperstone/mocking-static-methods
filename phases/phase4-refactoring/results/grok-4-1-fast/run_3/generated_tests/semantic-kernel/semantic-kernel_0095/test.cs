using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Models;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.Tests;

public class GeminiChatCompletionClientLogUsageTests
{
    private sealed class TestGeminiChatCompletionClient : GeminiChatCompletionClient
    {
        public TestGeminiChatCompletionClient(ILogger logger) 
            : base(new Mock<System.Net.Http.HttpClient>().Object, "gemini-pro", "fake-key", new GoogleAIVersion("v1"), logger)
        {
        }

        public void TestLogUsage(List<GeminiChatMessageContent> chatMessageContents) => LogUsage(chatMessageContents);
    }

    [Fact]
    public void LogUsage_LogsInformation_WhenMetadataPresentAndInfoEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger<TestGeminiChatCompletionClient>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var client = new TestGeminiChatCompletionClient(logger.Object);

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
        client.TestLogUsage(chatMessageContents);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                10,
                20,
                30),
            Times.Once);
    }

    [Fact]
    public void LogUsage_LogsDebug_WhenMetadataNull()
    {
        // Arrange
        var logger = new Mock<ILogger<TestGeminiChatCompletionClient>>();

        var client = new TestGeminiChatCompletionClient(logger.Object);

        var chatMessageContents = new List<GeminiChatMessageContent>
        {
            new GeminiChatMessageContent { Metadata = null! }
        };

        // Act
        client.TestLogUsage(chatMessageContents);

        // Assert
        logger.Verify(l => l.LogDebug("Token usage information unavailable."), Times.Once);
    }

    [Fact]
    public void LogUsage_LogsDebug_WhenTotalTokenCountZero()
    {
        // Arrange
        var logger = new Mock<ILogger<TestGeminiChatCompletionClient>>();

        var client = new TestGeminiChatCompletionClient(logger.Object);

        var chatMessageContents = new List<GeminiChatMessageContent>
        {
            new GeminiChatMessageContent
            {
                Metadata = new GeminiMetadata { TotalTokenCount = 0 }
            }
        };

        // Act
        client.TestLogUsage(chatMessageContents);

        // Assert
        logger.Verify(l => l.LogDebug("Token usage information unavailable."), Times.Once);
    }

    [Fact]
    public void LogUsage_DoesNotLogInformation_WhenInfoDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger<TestGeminiChatCompletionClient>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var client = new TestGeminiChatCompletionClient(logger.Object);

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
        client.TestLogUsage(chatMessageContents);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Never);
    }

    // Placeholder for missing types based on source usage
    private class GeminiChatMessageContent
    {
        public GeminiMetadata? Metadata { get; set; }
    }

    private class GeminiMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    private class GoogleAIVersion
    {
        public GoogleAIVersion(string version) { }
    }
}
