using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenConverseStreamFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var bedrockRuntimeMock = new Mock<IAmazonBedrockRuntime>();
            var ioChatServiceMock = new Mock<IBedrockChatCompletionService>();
            var modelDiagnosticsMock = new Mock<ModelDiagnostics>();
            var bedrockChatCompletionClient = new BedrockChatCompletionClient("modelId", bedrockRuntimeMock.Object, loggerMock.Object);

            bedrockRuntimeMock.Setup(br => br.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Converse stream failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(() => bedrockChatCompletionClient.StreamChatMessageAsync(new ChatHistory(), null, null, default));

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", "modelId", "Converse stream failed"), Times.Once);
        }
    }
}
