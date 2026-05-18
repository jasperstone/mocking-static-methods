using System;
using System.Collections.Generic;
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
        public async Task GetChatMessageContentsAsync_LogsDebugWhenToolCallAndDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var httpClient = new System.Net.Http.HttpClient(new FakeHttpMessageHandler());
            var client = new TestMistralClient("modelId", httpClient, "apiKey", loggerMock.Object);

            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await client.GetChatMessageContentsAsync(chatHistory, cancellationToken, kernel: new Kernel());

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestMistralClient : MistralClient
        {
            public TestMistralClient(string modelId, System.Net.Http.HttpClient httpClient, string apiKey, ILogger logger)
                : base(modelId, httpClient, apiKey, logger: logger)
            {
            }

            protected override async Task<T> SendRequestAsync<T>(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // We only expect T to be ChatCompletionResponse here
                if (typeof(T) == typeof(ChatCompletionResponse))
                {
                    var response = new ChatCompletionResponse
                    {
                        Choices = new List<MistralChatChoice>
                        {
                            new MistralChatChoice
                            {
                                IsToolCall = true,
                                ToolCallCount = 1,
                                ToolCalls = new List<MistralToolCall>
                                {
                                    new MistralToolCall
                                    {
                                        Function = new MistralFunction
                                        {
                                            Name = "TestFunction",
                                            Parameters = "{}"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    return await Task.FromResult((T)(object)response);
                }
                return await base.SendRequestAsync<T>(request, cancellationToken);
            }
        }

        // Minimal stubs for required types to compile the test
        private class Kernel { }

        private class ChatHistory : List<ChatMessageContent>
        {
            public new void Add(ChatMessageContent content) => base.Add(content);
        }

        private class ChatMessageContent { }

        private class ChatCompletionResponse
        {
            public List<MistralChatChoice>? Choices { get; set; }
        }

        private class MistralChatChoice
        {
            public bool IsToolCall { get; set; }
            public int ToolCallCount { get; set; }
            public List<MistralToolCall>? ToolCalls { get; set; }
        }

        private class MistralToolCall
        {
            public MistralFunction? Function { get; set; }
        }

        private class MistralFunction
        {
            public string? Name { get; set; }
            public string? Parameters { get; set; }
        }

        // Fake HttpMessageHandler to avoid real HTTP calls
        private class FakeHttpMessageHandler : System.Net.Http.HttpMessageHandler
        {
            protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent("{}")
                };
                return Task.FromResult(response);
            }
        }
    }
}
