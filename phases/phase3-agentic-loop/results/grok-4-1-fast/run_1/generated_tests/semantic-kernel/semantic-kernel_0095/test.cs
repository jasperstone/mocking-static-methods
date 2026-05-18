using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System.Reflection;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests;

public class GeminiChatCompletionClientLogUsageTests
{
    private class FakeGeminiChatMessageContent
    {
        public GeminiMetadata? Metadata { get; set; }
    }

    [Fact]
    public void LogUsage_LogsInformation_WhenMetadataPresentAndTokenCountPositive()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        
        var chatMessageContents = new List<FakeGeminiChatMessageContent>
        {
            new FakeGeminiChatMessageContent
            {
                Metadata = new GeminiMetadata
                {
                    PromptTokenCount = 10,
                    CandidatesTokenCount = 20,
                    TotalTokenCount = 30
                }
            }
        };

        var client = CreateClientWithLogger(mockLogger.Object);

        // Act
        var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(client, [chatMessageContents]);

        // Assert
        mockLogger.Verify(
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
        var mockLogger = new Mock<ILogger>();
        
        var chatMessageContents = new List<FakeGeminiChatMessageContent>
        {
            new FakeGeminiChatMessageContent { Metadata = null }
        };

        var client = CreateClientWithLogger(mockLogger.Object);

        // Act
        var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(client, [chatMessageContents]);

        // Assert
        mockLogger.Verify(l => l.LogDebug("Token usage information unavailable."), Times.Once);
    }

    [Fact]
    public void LogUsage_NoLogInformation_WhenInformationNotEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);
        
        var chatMessageContents = new List<FakeGeminiChatMessageContent>
        {
            new FakeGeminiChatMessageContent
            {
                Metadata = new GeminiMetadata
                {
                    PromptTokenCount = 10,
                    CandidatesTokenCount = 20,
                    TotalTokenCount = 30
                }
            }
        };

        var client = CreateClientWithLogger(mockLogger.Object);

        // Act
        var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(client, [chatMessageContents]);

        // Assert
        mockLogger.Verify(
            l => l.LogInformation(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public void LogUsage_NoLogInformation_WhenTotalTokenCountZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        
        var chatMessageContents = new List<FakeGeminiChatMessageContent>
        {
            new FakeGeminiChatMessageContent
            {
                Metadata = new GeminiMetadata
                {
                    PromptTokenCount = 0,
                    CandidatesTokenCount = 0,
                    TotalTokenCount = 0
                }
            }
        };

        var client = CreateClientWithLogger(mockLogger.Object);

        // Act
        var method = typeof(GeminiChatCompletionClient).GetMethod("LogUsage", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(client, [chatMessageContents]);

        // Assert
        mockLogger.Verify(
            l => l.LogInformation(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
        mockLogger.Verify(l => l.LogDebug("Token usage information unavailable."), Times.Once);
    }

    private static object CreateClientWithLogger(ILogger logger)
    {
        var httpClient = new Mock<HttpClient>().Object;
        
        var constructor = typeof(GeminiChatCompletionClient).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(HttpClient), typeof(string), typeof(string), typeof(GoogleAIVersion), typeof(ILogger) },
            null)!;
            
        return constructor.Invoke(new object[] { httpClient, "gemini-pro", "test-key", GoogleAIVersion.V1beta, logger });
    }
}
