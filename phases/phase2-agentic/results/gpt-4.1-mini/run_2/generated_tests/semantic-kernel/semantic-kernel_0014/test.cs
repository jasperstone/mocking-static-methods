using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsErrorOnException()
        {
            // Arrange
            var modelId = "test-model";
            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockChatService = new Mock<IBedrockChatCompletionService>();
            var mockConverseStreamRequest = new ConverseStreamRequest();
            var mockChatHistory = new ChatHistory();

            // Setup the service factory to return our mock chat service
            var serviceFactory = new BedrockServiceFactoryForTest(mockChatService.Object);

            // We need to inject the service factory or simulate it, but since the constructor creates it internally,
            // we will create a derived class to override the service creation for testing.
            var client = new TestBedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, mockLoggerFactory.Object, mockChatService.Object);

            // Setup the chat service to return a request
            mockChatService.Setup(s => s.GetConverseStreamRequest(modelId, mockChatHistory, null))
                .Returns(mockConverseStreamRequest);

            // Setup the bedrock runtime to throw an exception when ConverseStreamAsync is called
            var expectedException = new InvalidOperationException("Test exception");
            mockBedrockRuntime.Setup(r => r.ConverseStreamAsync(mockConverseStreamRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Setup the bedrock runtime to return a dummy endpoint URL
            mockBedrockRuntime.Setup(r => r.DetermineServiceOperationEndpoint(mockConverseStreamRequest))
                .Returns(new Amazon.BedrockRuntime.Model.Endpoint { URL = "https://dummyendpoint" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var _ in client.StreamChatMessageAsync(mockChatHistory))
                {
                    // Should not reach here
                }
            });

            Assert.Equal(expectedException, ex);

            // Verify that LogError was called with the expected parameters
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't converse stream with")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper derived class to override service factory creation
        private class TestBedrockChatCompletionClient : BedrockChatCompletionClient
        {
            private readonly IBedrockChatCompletionService _chatService;

            public TestBedrockChatCompletionClient(string modelId, IAmazonBedrockRuntime bedrockRuntime, ILoggerFactory? loggerFactory, IBedrockChatCompletionService chatService)
                : base(modelId, bedrockRuntime, loggerFactory)
            {
                _chatService = chatService;
            }

            // Override the _ioChatService field to use the injected mock
            protected override IBedrockChatCompletionService _ioChatService => _chatService;
        }

        // Dummy classes to satisfy dependencies
        private class ChatHistory { }
        private class ConverseStreamRequest { }
    }
}
