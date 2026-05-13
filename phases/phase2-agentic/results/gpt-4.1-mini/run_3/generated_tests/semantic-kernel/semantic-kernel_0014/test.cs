using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using Xunit;
using Amazon.BedrockRuntime.Model;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_LogsErrorOnException()
        {
            // Arrange
            var modelId = "test-model";
            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockChatService = new Mock<IBedrockChatCompletionService>();
            var chatHistory = new ChatHistory(new List<ChatMessage>()); // Assuming empty chat history is allowed for test

            // Setup the service factory to return our mock chat service
            var serviceFactory = new Mock<BedrockServiceFactory>();
            // We cannot mock constructor internals easily, so we will create a derived class to override the service creation
            var client = new TestBedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, mockLoggerFactory.Object, mockChatService.Object);

            // Setup the chat service to return a ConverseRequest
            var converseRequest = new ConverseRequest();
            mockChatService.Setup(s => s.GetConverseRequest(modelId, chatHistory, null)).Returns(converseRequest);

            // Setup the bedrock runtime to return an endpoint URL
            var endpoint = new Uri("https://test.endpoint");
            mockBedrockRuntime.Setup(r => r.DetermineServiceOperationEndpoint(converseRequest))
                .Returns(new Amazon.BedrockRuntime.Model.Endpoint { URL = endpoint.ToString() });

            // Setup the bedrock runtime ConverseAsync to throw an exception to trigger the catch block
            var testException = new InvalidOperationException("Test exception");
            mockBedrockRuntime.Setup(r => r.ConverseAsync(converseRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(testException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                client.GenerateChatMessageAsync(chatHistory));

            // Verify that LogError was called with the exception and expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't converse with")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper derived class to inject mock chat service
        private class TestBedrockChatCompletionClient : BedrockChatCompletionClient
        {
            private readonly IBedrockChatCompletionService _mockChatService;

            public TestBedrockChatCompletionClient(string modelId, IAmazonBedrockRuntime bedrockRuntime, ILoggerFactory? loggerFactory, IBedrockChatCompletionService mockChatService)
                : base(modelId, bedrockRuntime, loggerFactory)
            {
                _mockChatService = mockChatService;
            }

            // Override the _ioChatService property to return the mock
            protected override IBedrockChatCompletionService _ioChatService => _mockChatService;
        }
    }

    // Minimal stubs for dependent types to allow compilation
    internal class ChatHistory : List<ChatMessage>
    {
        public ChatHistory(IEnumerable<ChatMessage> messages) : base(messages) { }
    }

    internal class ChatMessage { }

    internal class ConverseRequest { }

    internal class Endpoint
    {
        public string URL { get; set; } = string.Empty;
    }
}
