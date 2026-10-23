using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.UnitTests.Client;

public class MistralClientLoggerTests
{
    [Fact]
    public async Task GetChatMessageContentsAsync_LogsDebugMessage_WhenToolCallPresentAndDebugEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Capture Log calls to verify the specific LogDebug message
        mockLogger.Setup(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Tool requests:") && state.ToString()!.Contains("{Requests}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var httpClient = new Mock<HttpClient>().Object;
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");

        var client = new MistralClient("test-model", httpClient, "test-key", logger: mockLogger.Object);

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None);

        // Assert - Verify the specific LogDebug call on line 128 was made
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Tool requests: {Requests}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_DoesNotLogDebug_WhenDebugDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        var httpClient = new Mock<HttpClient>().Object;
        var chatHistory = new ChatHistory();

        var client = new MistralClient("test-model", httpClient, "test-key", logger: mockLogger.Object);

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None);

        // Assert - No debug logs when debug is disabled
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_IsEnabledCheckPreventsLog_WhenDebugEnabledButNoToolCalls()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var httpClient = new Mock<HttpClient>().Object;
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");

        var client = new MistralClient("test-model", httpClient, "test-key", logger: mockLogger.Object);

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None);

        // Assert - The IsEnabled check prevents the LogDebug call from three paths:
        // 1. !autoInvoke, 2. Choices.Count != 1, 3. !chatChoice.IsToolCall
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Tool requests:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
