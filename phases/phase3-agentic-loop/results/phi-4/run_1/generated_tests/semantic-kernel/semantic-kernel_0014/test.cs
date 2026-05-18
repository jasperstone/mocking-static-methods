using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
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
            var modelId = "test-model-id";
            var chatHistory = new ChatHistory();
            var client = new BedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, Mock.Of<ILoggerFactory>(lf => lf.CreateLogger(typeof(BedrockChatCompletionClient)) == mockLogger.Object))
            {
                _ioChatService = mockChatService.Object
            };

            mockBedrockRuntime.Setup(br => br.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory));

            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Can't converse stream with '{ModelId}'. Reason: {Error}",
                    modelId,
                    It.Is<string>(s => s == "Test exception")),
                Times.Once);
        }
    }
}
