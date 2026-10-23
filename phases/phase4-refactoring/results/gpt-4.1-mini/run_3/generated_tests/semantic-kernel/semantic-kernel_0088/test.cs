using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    // This test class assumes it is in the same assembly or that InternalsVisibleTo is set
    public class GeminiChatCompletionClientInternalTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()));

            var httpClient = new System.Net.Http.HttpClient();

            var client = new GeminiChatCompletionClient(
                httpClient,
                modelId: "test-model",
                apiKey: "test-key",
                apiVersion: GoogleAIVersion.V1,
                logger: loggerMock.Object);

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

            // Use reflection to invoke private method ProcessFunctionsAsync
            var method = typeof(GeminiChatCompletionClient).GetMethod("ProcessFunctionsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            var task = (Task)method.Invoke(client, new object[] { state, CancellationToken.None })!;
            await task;

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tool requests: 2")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()), Times.Once);
        }
    }

    // Minimal stubs for types used in the test
    internal class ChatCompletionState
    {
        public GeminiChatMessage? LastMessage { get; set; }
        public bool AutoInvoke { get; set; }
        public bool FilterTerminationRequested { get; set; }
    }

    internal class GeminiChatMessage
    {
        public List<GeminiToolCall>? ToolCalls { get; set; }
    }

    internal class GeminiToolCall
    {
        public override string ToString() => "ToolCall";
    }

    internal enum GoogleAIVersion
    {
        V1
    }
}
