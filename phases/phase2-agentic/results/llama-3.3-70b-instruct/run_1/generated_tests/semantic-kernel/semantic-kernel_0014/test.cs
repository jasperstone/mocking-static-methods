using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;

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
            var modelId = "model-id";
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();
            var cancellationToken = default;

            bedrockRuntimeMock
                .Setup(br => br.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Converse stream failed"));

            var client = new BedrockChatCompletionClient(modelId, bedrockRuntimeMock.Object, new LoggerFactory().CreateLogger<BedrockChatCompletionClient>());

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", modelId, It.IsAny<string>()), Times.Once);
        }
    }
}
