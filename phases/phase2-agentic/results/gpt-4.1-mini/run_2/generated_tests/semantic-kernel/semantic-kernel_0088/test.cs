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
        public async Task ProcessFunctionsAsync_LogsToolRequestsCount_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            var client = new GeminiChatCompletionClient(
                httpClient: new System.Net.Http.HttpClient(),
                modelId: "test-model",
                apiKey: "test-api-key",
                apiVersion: GoogleAIVersion.V1,
                logger: loggerMock.Object);

            // Create a ChatCompletionState with a LastMessage containing ToolCalls
            var toolCalls = new List<ToolCall>
            {
                new ToolCall("tool1"),
                new ToolCall("tool2"),
                new ToolCall("tool3")
            };

            var lastMessage = new ChatMessage
            {
                ToolCalls = toolCalls
            };

            var state = new ChatCompletionState
            {
                LastMessage = lastMessage
            };

            // Use reflection to invoke the private method ProcessFunctionsAsync
            var method = typeof(GeminiChatCompletionClient).GetMethod("ProcessFunctionsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            var task = (Task)method.Invoke(client, new object[] { state, CancellationToken.None })!;
            await task;

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tool requests: 3")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Minimal stubs for dependent types to allow compilation and testing
    internal class ChatCompletionState
    {
        public ChatMessage LastMessage { get; set; } = null!;
        public bool AutoInvoke { get; set; }
        public bool FilterTerminationRequested { get; set; }
    }

    internal class ChatMessage
    {
        public List<ToolCall>? ToolCalls { get; set; }
    }

    internal class ToolCall
    {
        private readonly string _name;

        public ToolCall(string name)
        {
            _name = name;
        }

        public override string ToString() => _name;
    }

    internal enum GoogleAIVersion
    {
        V1
    }
}
