using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Microsoft.SemanticKernel.Text;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_LogsDebugWhenToolCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var httpClient = new System.Net.Http.HttpClient(new FakeHttpMessageHandler());
            var client = new MistralClient("modelId", httpClient, "apiKey", logger: loggerMock.Object);

            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;

            // Setup a fake response with a tool call to trigger the LogDebug call
            var responseData = new ChatCompletionResponse
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
                                Function = new MistralFunction { Name = "func", Parameters = "{}" }
                            }
                        }
                    }
                }
            };

            // We need to mock SendRequestAsync to return our responseData
            var clientMock = new Mock<MistralClient>("modelId", httpClient, "apiKey", null) { CallBase = true };
            clientMock
                .Setup(c => c.SendRequestAsync<ChatCompletionResponse>(It.IsAny<System.Net.Http.HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseData);

            // Act
            var result = await clientMock.Object.GetChatMessageContentsAsync(chatHistory, cancellationToken, kernel: new Kernel());

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);
        }

        // Minimal stubs for dependencies to compile
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

        private class ChatHistory : List<ChatMessageContent>
        {
            public void Add(ChatMessageContent content) => base.Add(content);
        }

        private class Kernel : Microsoft.SemanticKernel.Kernel
        {
            // Minimal stub
        }

        private class ChatCompletionResponse
        {
            public List<MistralChatChoice>? Choices { get; set; }
        }

        private class MistralChatChoice
        {
            public bool IsToolCall { get; set; }
            public int ToolCallCount => ToolCalls?.Count ?? 0;
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

        private class ChatMessageContent
        {
        }
    }
}
