using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;

namespace MistralClientTests
{
    public class MistralClientLoggingTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolCallIsDetected()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MistralClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var client = new TestMistralClient(mockHttpClient.Object, mockLogger.Object);

            var chatHistory = new List<ChatMessageContent>();

            // Setup a fake response with a tool call
            var fakeResponse = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice>
                {
                    new MistralChatChoice
                    {
                        IsToolCall = true,
                        ToolCallCount = 1,
                        ToolCalls = new List<ToolCall>
                        {
                            new ToolCall
                            {
                                Function = new Function { Name = "TestFunction", Parameters = "{}" }
                            }
                        }
                    }
                },
                Usage = new MistralUsage { PromptTokens = 5, CompletionTokens = 10 }
            };

            // Override SendRequestAsync to return the fake response
            var testClient = new TestMistralClient(mockHttpClient.Object, mockLogger.Object, fakeResponse);

            // Act
            await testClient.GetChatMessageContentsAsync(chatHistory, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Subclass to override methods for testing
        private class TestMistralClient : MistralClient
        {
            private readonly ChatCompletionResponse _response;

            public TestMistralClient(HttpClient httpClient, ILogger<MistralClient> logger, ChatCompletionResponse response = null)
                : base("test-model", httpClient, "test-api-key", logger: logger)
            {
                _response = response;
            }

            protected override Task<T> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult((T)(object)_response);
            }

            protected override List<ChatMessageContent> ToChatMessageContent(string modelId, ChatCompletionResponse responseData, MistralChatChoice? choice = null)
            {
                return new List<ChatMessageContent> { new ChatMessageContent("test") };
            }
        }
    }
}
