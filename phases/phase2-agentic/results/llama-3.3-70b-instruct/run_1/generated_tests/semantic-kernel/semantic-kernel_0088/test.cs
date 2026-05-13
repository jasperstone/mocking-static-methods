using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var chatCompletionState = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new[] { new ToolCall() }
                }
            };

            var geminiChatCompletionClient = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            // Act
            await geminiChatCompletionClient.ProcessFunctionsAsync(chatCompletionState, CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Tool requests: {Requests}", 1), Times.Once);
        }
    }
}
