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
            var bedrockChatCompletionClient = new BedrockChatCompletionClient("modelId", Mock.Of<IAmazonBedrockRuntime>(), new LoggerFactory().AddMock(loggerMock.Object));
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();
            var cancellationToken = new CancellationToken();

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => bedrockChatCompletionClient.StreamChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken));
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", "modelId", It.IsAny<string>()), Times.Once);
        }
    }
}
