using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
using Moq;
using Xunit;

[assembly: InternalsVisibleTo("Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests")]

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
            var kernel = new Kernel();
            var cancellationToken = new CancellationToken();

            var bedrockRuntimeMock = new Mock<IAmazonBedrockRuntime>();
            var ioChatServiceMock = new Mock<IBedrockChatCompletionService>();
            var loggerMock = new Mock<ILogger>();

            var client = new BedrockChatCompletionClient(modelId, bedrockRuntimeMock.Object, new LoggerFactory().AddProvider(new NullLoggerProvider()));

            var exception = new Exception("Test exception");
            bedrockRuntimeMock.Setup(x => x.ConverseStreamAsync(It.IsAny<ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            var exceptionThrown = await Record.ExceptionAsync(async () =>
            {
                await foreach (var message in client.StreamChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken))
                {
                    // Do nothing
                }
            });

            // Assert
            Assert.NotNull(exceptionThrown);
            Assert.IsType<Exception>(exceptionThrown);
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
