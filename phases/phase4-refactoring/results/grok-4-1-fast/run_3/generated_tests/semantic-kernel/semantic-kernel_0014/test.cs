using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task StreamChatMessageAsync_LogsError_OnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var modelId = "test-model-id";
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage("test");

            mockBedrockRuntime
                .Setup(x => x.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);

            var constructor = typeof(BedrockChatCompletionClient)
                .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(string), typeof(IAmazonBedrockRuntime), typeof(ILoggerFactory) }, null)!;

            var client = constructor.Invoke(new object[] { modelId, mockBedrockRuntime.Object, loggerFactory.Object });

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(
                () => CollectAsyncEnumerable(((dynamic)client).StreamChatMessageAsync(chatHistory)));

            // Verify LogError was called with correct parameters
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Can't converse stream with '{ModelId}'. Reason: {Error}",
                    modelId,
                    "Test exception"),
                Times.Once);
        }

        private static async Task CollectAsyncEnumerable<T>(IAsyncEnumerable<T> enumerable)
        {
            await foreach (var _ in enumerable)
            {
            }
        }
    }
}
