using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolCallAndDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var httpClient = new HttpClient(new FakeHttpMessageHandler());

            var client = new MistralClient("modelId", httpClient, "apiKey", logger: loggerMock.Object);

            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            // We expect an exception because the fake HTTP response is empty and deserialization will fail,
            // but we want to verify that the logger's LogDebug was called if possible.
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await client.GetChatMessageContentsAsync(chatHistory, cancellationToken, kernel: new Kernel());
            });

            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        // Minimal stubs for required types
        private class ChatHistory : List<ChatMessageContent>
        {
        }

        private class Kernel
        {
        }

        private class ChatMessageContent
        {
        }

        // Fake HttpMessageHandler to avoid real HTTP calls
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Return a JSON response that will cause the client to parse a single tool call choice
                string jsonResponse = @"
                {
                    ""choices"": [
                        {
                            ""isToolCall"": true,
                            ""toolCallCount"": 1,
                            ""toolCalls"": [
                                {
                                    ""function"": {
                                        ""name"": ""TestFunction"",
                                        ""parameters"": ""{}""
                                    }
                                }
                            ]
                        }
                    ]
                }";

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                };
                return Task.FromResult(response);
            }
        }
    }
}
