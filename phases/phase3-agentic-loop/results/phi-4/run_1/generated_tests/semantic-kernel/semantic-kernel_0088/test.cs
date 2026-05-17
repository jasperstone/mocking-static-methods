using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion; // Namespace for ChatCompletionState, ChatMessage, and ToolCall
using Microsoft.SemanticKernel.Connectors.Google.Core; // Namespace for GeminiChatCompletionClient

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new HttpClient(),
                "model-id",
                "api-key",
                GoogleAIVersion.V1,
                mockLogger.Object);

            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<ToolCall> { new ToolCall(), new ToolCall() }
                }
            };

            // Act
            await client.ProcessFunctionsAsync(state, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(s => s == "Tool requests: {Requests}"),
                    It.Is<int>(count => count == 2)),
                Times.Once);
        }
    }
}
