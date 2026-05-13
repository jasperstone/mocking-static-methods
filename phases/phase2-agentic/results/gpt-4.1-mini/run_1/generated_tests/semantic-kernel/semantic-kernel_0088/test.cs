using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var client = new TestableGeminiChatCompletionClient(loggerMock.Object);

            var state = new ChatCompletionState
            {
                LastMessage = new GeminiChatMessage
                {
                    ToolCalls = new List<GeminiToolCall>
                    {
                        new GeminiToolCall(),
                        new GeminiToolCall()
                    }
                }
            };

            // Act
            await client.InvokeProcessFunctionsAsync(state, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Tool requests: 2"),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);
        }

        // Helper classes to expose the private method for testing
        private class TestableGeminiChatCompletionClient : GeminiChatCompletionClient
        {
            public TestableGeminiChatCompletionClient(ILogger logger)
                : base(
                    new System.Net.Http.HttpClient(),
                    "modelId",
                    "apiKey",
                    GoogleAIVersion.V1,
                    logger)
            {
            }

            public async Task InvokeProcessFunctionsAsync(ChatCompletionState state, CancellationToken cancellationToken)
            {
                await this.ProcessFunctionsAsync(state, cancellationToken);
            }
        }

        // Minimal stubs for dependencies
        private class ChatCompletionState
        {
            public GeminiChatMessage LastMessage { get; set; } = null!;
            public bool AutoInvoke { get; set; }
            public bool FilterTerminationRequested { get; set; }
        }

        private class GeminiChatMessage
        {
            public List<GeminiToolCall>? ToolCalls { get; set; }
        }

        private class GeminiToolCall
        {
            public override string ToString() => "ToolCall";
        }

        private enum GoogleAIVersion
        {
            V1
        }
    }
}
