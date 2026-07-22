using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client
{
    public class MistralClientLoggingTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_LogsToolRequests_WhenDebugEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var httpClient = new HttpClient(new FakeHttpMessageHandler());

            // Use reflection to create instance of internal MistralClient
            var mistralClientType = typeof(MistralClient);
            var ctor = mistralClientType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { typeof(string), typeof(HttpClient), typeof(string), typeof(Uri), typeof(ILogger) },
                null);
            Assert.NotNull(ctor);

            var client = (MistralClient)ctor.Invoke(new object[] { "modelId", httpClient, "apiKey", null, mockLogger.Object });

            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                // Invoke internal async method via reflection
                var method = mistralClientType.GetMethod("GetChatMessageContentsAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Assert.NotNull(method);

                var task = (Task<IReadOnlyList<ChatMessageContent>>)method.Invoke(client, new object[] { chatHistory, cancellationToken, null, null });
                await task;
            });

            // Verify that LogDebug was called with the expected message template
            mockLogger.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Minimal stub classes to allow compilation
        private class ChatHistory : List<ChatMessageContent>
        {
        }

        private class ChatMessageContent
        {
        }

        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var json = @"
                {
                    ""choices"": [
                        {
                            ""toolCallCount"": 1,
                            ""isToolCall"": true,
                            ""toolCalls"": [
                                {
                                    ""function"": {
                                        ""name"": ""func1"",
                                        ""parameters"": ""param1""
                                    }
                                }
                            ]
                        }
                    ]
                }";
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                };
                return Task.FromResult(response);
            }
        }
    }
}
