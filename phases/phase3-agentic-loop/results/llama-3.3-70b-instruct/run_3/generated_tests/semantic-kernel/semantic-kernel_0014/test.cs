using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenConverseStreamAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var bedrockRuntimeMock = new Mock<IAmazonBedrockRuntime>();
            var ioChatServiceMock = new Mock<IBedrockChatCompletionService>();
            var bedrockChatCompletionClient = new BedrockChatCompletionClient("modelId", bedrockRuntimeMock.Object, new LoggerFactory().CreateLogger<BedrockChatCompletionClient>());

            bedrockRuntimeMock.Setup(br => br.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("ConverseStreamAsync failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => bedrockChatCompletionClient.StreamChatMessageAsync(new ChatHistory(), null, null, default));

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", "modelId", "ConverseStreamAsync failed"), Times.Once);
        }
    }
}
