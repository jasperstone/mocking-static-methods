using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;
using System.Reflection;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests;

public class MistralClientLoggerTests
{
    [Fact]
    public async Task LogDebugToolRequests_IsCalled_WhenDebugEnabled_AndToolCallPresent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler
            .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetToolCallResponseJson(2), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var client = CreateClient(httpClient, mockLogger.Object);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act
        await InvokeGetChatMessageContentsAsync(client, chatHistory, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug("Tool requests: {Requests}", 2),
            Times.Once);
    }

    [Fact]
    public async Task LogDebugToolRequests_IsNotCalled_WhenDebugDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler
            .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetToolCallResponseJson(1), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var client = CreateClient(httpClient, mockLogger.Object);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act
        await InvokeGetChatMessageContentsAsync(client, chatHistory, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task LogDebugToolRequests_IsNotCalled_WhenNoToolCall()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler
            .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetNoToolCallResponseJson(), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var client = CreateClient(httpClient, mockLogger.Object);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act
        await InvokeGetChatMessageContentsAsync(client, chatHistory, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug("Tool requests: {Requests}", It.IsAny<int>()),
            Times.Never);
    }

    private static MistralClient CreateClient(HttpClient httpClient, ILogger logger)
    {
        var constructor = typeof(MistralClient).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(string), typeof(HttpClient), typeof(string), typeof(Uri), typeof(ILogger) },
            null)!;

        return (MistralClient)constructor.Invoke(new object?[] { "model123", httpClient, "api-key", null, logger });
    }

    private static async Task InvokeGetChatMessageContentsAsync(MistralClient client, ChatHistory chatHistory, CancellationToken cancellationToken)
    {
        var method = typeof(MistralClient).GetMethod(
            "GetChatMessageContentsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(ChatHistory), typeof(CancellationToken), typeof(object), typeof(object) },
            null)!;

        await (Task)method.Invoke(client, new object?[] { chatHistory, cancellationToken, null, null })!;
    }

    private static string GetToolCallResponseJson(int toolCallCount)
    {
        var toolCalls = string.Join(",", Enumerable.Range(1, toolCallCount).Select(i => 
            $$"""{"index": {{i-1}}, "id": "call{{i}}", "type": "function", "function": {"name": "func{{i}}", "arguments": "{}"}}"""));
        
        return $$"""
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "choices": [{
                "index": 0,
                "message": {
                    "role": "assistant",
                    "content": null,
                    "tool_calls": [{{toolCalls}}]
                },
                "finish_reason": "tool_calls"
            }],
            "usage": {}
        }
        """;
    }

    private static string GetNoToolCallResponseJson()
    {
        return """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "choices": [{
                "index": 0,
                "message": {
                    "role": "assistant",
                    "content": "Hello!"
                },
                "finish_reason": "stop"
            }],
            "usage": {}
        }
        """;
    }
}
