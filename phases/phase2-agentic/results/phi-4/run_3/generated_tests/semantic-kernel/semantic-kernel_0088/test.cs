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
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenLogLevelIsDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
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

            // Set up the mock to expect a LogDebug call
            mockLogger
                .Setup(l => l.IsEnabled(LogLevel.Debug))
                .Returns(true);

            mockLogger
                .Setup(l => l.LogDebug(
                    It.Is<string>(s => s == "Tool requests: {Requests}"),
                    It.Is<int>(i => i == 2)))
                .Verifiable();

            // Act
            await client.ProcessFunctionsAsync(state, CancellationToken.None);

            // Assert
            mockLogger.Verify();
        }
    }
}
