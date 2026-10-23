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
        public async Task StreamChatMessageAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var modelId = "test-model";
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var cancellationToken = CancellationToken.None;

            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockIoChatService = new Mock<IBedrockChatCompletionService>();
            var mockLogger = new Mock<ILogger<BedrockChatCompletionClient>>();

            mockIoChatService.Setup(service => service.GetConverseStreamRequest(modelId, chatHistory, executionSettings))
                .Returns(new ConverseStreamRequest());

            mockBedrockRuntime.Setup(runtime => runtime.DetermineServiceOperationEndpoint(It.IsAny<ConverseStreamRequest>()))
                .Returns(new ServiceOperationEndpoint { URL = "http://test-url" });

            mockBedrockRuntime.Setup(runtime => runtime.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), cancellationToken))
                .ThrowsAsync(new Exception("Test exception"));

            var client = new BedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, Mock.Of<ILoggerFactory>());

            // Act
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, executionSettings, null, cancellationToken).ToListAsync());

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
