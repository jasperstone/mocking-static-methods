using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Connectors.Amazon.Bedrock.Core.Clients.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var bedrockRuntimeMock = new Mock<IBedrockRuntime>();
            var ioChatServiceMock = new Mock<IIoChatService>();
            var modelId = "test-model-id";
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var client = new BedrockChatCompletionClient(loggerMock.Object, bedrockRuntimeMock.Object, ioChatServiceMock.Object, modelId);

            // Simulate an exception
            bedrockRuntimeMock
                .Setup(b => b.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, executionSettings));

            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Can't converse stream with '{ModelId}'. Reason: {Error}",
                    modelId,
                    It.Is<string>(s => s == "Test exception")),
                Times.Once);
        }
    }
}
