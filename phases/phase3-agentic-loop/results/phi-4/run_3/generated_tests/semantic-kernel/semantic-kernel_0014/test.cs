using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockChatService = new Mock<IBedrockChatCompletionService>();

            var client = new BedrockChatCompletionClient(
                "modelId",
                mockBedrockRuntime.Object,
                new LoggerFactory().AddProvider(new MockProvider(mockLogger.Object))
            );

            mockBedrockRuntime
                .Setup(b => b.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;

            // Act
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, null, null, cancellationToken));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Can't converse stream with '{ModelId}'. Reason: {Error}",
                    "modelId",
                    It.Is<string>(s => s == "Test exception")
                ),
                Times.Once
            );
        }
    }

    public class MockProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
