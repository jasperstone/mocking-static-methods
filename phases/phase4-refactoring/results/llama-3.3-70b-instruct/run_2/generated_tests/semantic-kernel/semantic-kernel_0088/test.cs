using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsToolRequests()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var client = new GeminiChatCompletionClient(
                new HttpClient(),
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<ToolCall>
                    {
                        new ToolCall(),
                        new ToolCall(),
                        new ToolCall()
                    }
                }
            };

            // Act
            await client.ProcessFunctionsAsync(state, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", 3), Times.Once);
        }
    }
}
