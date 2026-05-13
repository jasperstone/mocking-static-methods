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

            var bedrockRuntimeMock = new Mock<IAmazonBedrockRuntime>();
            bedrockRuntimeMock
                .Setup(r => r.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var loggerMock = new Mock<ILogger<BedrockChatCompletionClient>>();

            var client = new BedrockChatCompletionClient(modelId, bedrockRuntimeMock.Object, Mock.Of<ILoggerFactory>());

            // Act
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, executionSettings, null, cancellationToken).ToListAsync());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
