using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenConverseStreamFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BedrockChatCompletionClient>>();
            var bedrockChatCompletionClient = new BedrockChatCompletionClient("modelId", null, loggerMock.Object);
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<Exception>(() => bedrockChatCompletionClient.StreamChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken));

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", "modelId", It.IsAny<string>()), Times.Once);
        }
    }
}
