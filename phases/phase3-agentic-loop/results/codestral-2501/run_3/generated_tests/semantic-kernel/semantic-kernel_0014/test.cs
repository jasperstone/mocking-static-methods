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
        public async Task StreamChatMessageAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var modelId = "test-model";
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var cancellationToken = new CancellationToken();

            var bedrockRuntimeMock = new Mock<IAmazonBedrockRuntime>();
            var ioChatServiceMock = new Mock<IBedrockChatCompletionService>();
            var loggerMock = new Mock<ILogger<BedrockChatCompletionClient>>();

            var converseStreamRequest = new ConverseStreamRequest();
            var exception = new Exception("Test exception");

            bedrockRuntimeMock
                .Setup(r => r.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            ioChatServiceMock
                .Setup(s => s.GetConverseStreamRequest(It.IsAny<string>(), It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings>()))
                .Returns(converseStreamRequest);

            var client = new BedrockChatCompletionClient(modelId, bedrockRuntimeMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => client.StreamChatMessageAsync(chatHistory, executionSettings, null, cancellationToken).ToListAsync());

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't converse stream with 'test-model'. Reason: Test exception")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
