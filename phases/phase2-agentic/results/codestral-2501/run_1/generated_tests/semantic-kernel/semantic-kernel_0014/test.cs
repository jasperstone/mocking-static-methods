using System;
using System.Collections.Generic;
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
        public async Task StreamChatMessageAsync_ThrowsException_LogsError()
        {
            // Arrange
            var modelId = "test-model";
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var cancellationToken = CancellationToken.None;

            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockIoChatService = new Mock<IBedrockChatCompletionService>();
            var mockLogger = new Mock<ILogger>();

            var converseStreamRequest = new ConverseStreamRequest();
            mockIoChatService.Setup(service => service.GetConverseStreamRequest(modelId, chatHistory, executionSettings))
                .Returns(converseStreamRequest);

            var exception = new Exception("Test exception");
            mockBedrockRuntime.Setup(runtime => runtime.ConverseStreamAsync(converseStreamRequest, cancellationToken))
                .ThrowsAsync(exception);

            var client = new BedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, Mock.Of<ILoggerFactory>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, executionSettings, null, cancellationToken).ToListAsync());
            mockLogger.Verify(logger => logger.LogError(exception, "Can't converse stream with '{ModelId}'. Reason: {Error}", modelId, exception.Message), Times.Once);
        }
    }
}
