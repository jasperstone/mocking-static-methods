using System;
using System.Collections.Generic;
using System.Linq;
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
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.UnitTests;

public class MistralClientLoggerTests
{
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responseFactory(request, cancellationToken);
        }
    }

    [Fact]
    public async Task LogsDebugMessage_WhenToolCallPresentAndDebugEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var responseJson = """
        {
            "choices": [{
                "tool_calls": [{
                    "function": { "name": "test", "arguments": "{}" }
                }]
            }]
        }
        """;
        
        var httpClient = new HttpClient(new FakeHttpMessageHandler((req, ct) => 
            Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            })));

        var chatHistory = new ChatHistory();
        var mockKernel = new Mock<Kernel>();
        
        // Use reflection to create internal MistralClient
        var client = CreateMistralClient("test-model", httpClient, "test-key", mockLogger.Object);

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None, null, mockKernel.Object);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<object[]>(args => args.Length == 1 && args[0] is int),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DoesNotLogDebug_WhenDebugDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        var responseJson = """
        {
            "choices": [{
                "tool_calls": [{
                    "function": { "name": "test", "arguments": "{}" }
                }]
            }]
        }
        """;
        
        var httpClient = new HttpClient(new FakeHttpMessageHandler((req, ct) => 
            Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            })));

        var chatHistory = new ChatHistory();
        var mockKernel = new Mock<Kernel>();
        
        var client = CreateMistralClient("test-model", httpClient, "test-key", mockLogger.Object);

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None, null, mockKernel.Object);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task DoesNotLogDebug_WhenNoToolCall()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var responseJson = """
        {
            "choices": [{
                "message": { "content": "hello" }
            }]
        }
        """;
        
        var httpClient = new HttpClient(new FakeHttpMessageHandler((req, ct) => 
            Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            })));

        var chatHistory = new ChatHistory();
        var mockKernel = new Mock<Kernel>();
        
        var client = CreateMistralClient("test-model", httpClient, "test-key", mockLogger.Object);

        // Act
        await client.GetChatMessageContentsAsync(chatHistory, CancellationToken.None, null, mockKernel.Object);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<object[]>(args => args.Length == 1 && args[0] is int),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private static dynamic CreateMistralClient(string modelId, HttpClient httpClient, string apiKey, ILogger logger)
    {
        var type = Type.GetType("Microsoft.SemanticKernel.Connectors.MistralAI.Client.MistralClient, Microsoft.SemanticKernel.Connectors.MistralAI")!;
        return Activator.CreateInstance(type!, modelId, httpClient, apiKey, null, logger)!;
    }
}
