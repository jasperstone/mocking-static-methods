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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_WhenConverseStreamFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BedrockChatCompletionClient>>();
            var bedrockRuntimeMock = new Mock<IAmazonBedrockRuntime>();
            var ioChatServiceMock = new Mock<IBedrockChatCompletionService>();
            var modelId = "modelId";
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();
            var cancellationToken = default;

            bedrockRuntimeMock
                .Setup(br => br.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Converse stream failed"));

            var client = new BedrockChatCompletionClient(modelId, bedrockRuntimeMock.Object, new LoggerFactory().CreateLogger<BedrockChatCompletionClient>());

            // Act
            try
            {
                await foreach (var _ in client.StreamChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken))
                {
                }
            }
            catch (Exception ex)
            {
                // Assert
                loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Can't converse stream with '{ModelId}'. Reason: {Error}", modelId, ex.Message), Times.Once);
            }
        }
    }
}
