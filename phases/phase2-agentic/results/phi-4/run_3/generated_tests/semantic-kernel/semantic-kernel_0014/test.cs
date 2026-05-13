using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.Logging;
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
            var mockIoChatService = new Mock<IBedrockChatCompletionService>();
            var modelId = "test-model-id";
            var chatHistory = new ChatHistory();
            var client = new BedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, mockLogger.Object)
            {
                _ioChatService = mockIoChatService.Object
            };

            mockBedrockRuntime
                .Setup(b => b.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory));

            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Can't converse stream with '{ModelId}'. Reason: {Error}",
                    modelId,
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}
