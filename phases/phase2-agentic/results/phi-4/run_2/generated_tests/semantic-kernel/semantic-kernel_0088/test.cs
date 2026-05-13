using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var client = new GeminiChatCompletionClient(
                httpClient: new HttpClient(),
                modelId: "test-model-id",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: mockLogger.Object);

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
                l => l.LogDebug(
                    It.Is<string>(s => s.Contains("Tool requests: {Requests}")),
                    It.Is<object>(o => o is int && (int)o == 2)),
                Times.Once);
        }
    }
}
