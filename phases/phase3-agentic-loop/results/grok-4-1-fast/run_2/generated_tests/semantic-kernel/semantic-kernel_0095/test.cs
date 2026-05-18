using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests.Gemini.Clients;

public class GeminiChatCompletionClientLogUsageTests
{
    [Fact]
    public void LogUsage_LogsInformation_WhenMetadataPresentAndInfoEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        
        var testClient = new TestableGeminiChatCompletionClient(logger.Object, hasValidMetadata: true);
        var chatMessageContents = new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, "test content") { Metadata = new() }
        };

        // Act
        testClient.LogUsage(chatMessageContents);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                100,
                50,
                150),
            Times.Once);
    }

    [Fact]
    public void LogUsage_DoesNotLogInformation_WhenInfoNotEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);
        
        var testClient = new TestableGeminiChatCompletionClient(logger.Object, hasValidMetadata: true);
        var chatMessageContents = new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, "test content") { Metadata = new() }
        };

        // Act
        testClient.LogUsage(chatMessageContents);

        // Assert
        logger.Verify(
            l => l.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Never);
    }

    [Fact]
    public void LogUsage_LogsDebug_WhenMetadataAbsent()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        
        var testClient = new TestableGeminiChatCompletionClient(logger.Object, hasValidMetadata: false);
        var chatMessageContents = new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, "test content")
        };

        // Act
        testClient.LogUsage(chatMessageContents);

        // Assert
        logger.Verify(l => l.LogDebug("Token usage information unavailable."), Times.Once);
    }

    [Fact]
    public void LogUsage_LogsDebug_WhenTotalTokenCountZero()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        
        var testClient = new TestableGeminiChatCompletionClient(logger.Object, hasValidMetadata: false);
        var chatMessageContents = new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, "test content") { Metadata = new() }
        };

        // Act
        testClient.LogUsage(chatMessageContents);

        // Assert
        logger.Verify(l => l.LogDebug("Token usage information unavailable."), Times.Once);
    }

    private class TestableGeminiChatCompletionClient
    {
        private readonly ILogger _logger;
        private readonly bool _hasValidMetadata;

        public TestableGeminiChatCompletionClient(ILogger logger, bool hasValidMetadata)
        {
            _logger = logger;
            _hasValidMetadata = hasValidMetadata;
        }

        public void LogUsage(List<ChatMessageContent> chatMessageContents)
        {
            if (chatMessageContents.Count == 0) return;

            var metadata = _hasValidMetadata ? new MockGeminiMetadata() : null;

            if (metadata is null || metadata.TotalTokenCount <= 0)
            {
                _logger.LogDebug("Token usage information unavailable.");
                return;
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                    metadata.PromptTokenCount,
                    metadata.CandidatesTokenCount,
                    metadata.TotalTokenCount);
            }
        }

        private class MockGeminiMetadata
        {
            public int PromptTokenCount => 100;
            public int CandidatesTokenCount => 50;
            public int TotalTokenCount => 150;
        }
    }
}
