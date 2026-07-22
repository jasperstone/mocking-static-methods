using System.Collections.Generic;
using System.Net.Http;
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
        public async Task GenerateChatMessageAsync_LogsDebugMessage_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            loggerMock.Setup(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()));

            var client = new GeminiChatCompletionClient(
                new HttpClient(),
                "test-model",
                "test-api-key",
                GoogleAIVersion.V1,
                loggerMock.Object);

            var chatHistory = new ChatHistory
            {
                Messages = new List<GeminiChatMessage>
                {
                    new GeminiChatMessage
                    {
                        ToolCalls = new List<GeminiToolCall>
                        {
                            new GeminiToolCall(),
                            new GeminiToolCall()
                        }
                    }
                }
            };

            // Act
            // We call GenerateChatMessageAsync which internally calls ProcessFunctionsAsync
            // We pass a cancellation token that is not cancelled
            await client.GenerateChatMessageAsync(chatHistory, cancellationToken: CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 2")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()), Times.AtLeastOnce);
        }
    }

    // Minimal stubs for types used in the test
    internal class ChatHistory
    {
        public List<GeminiChatMessage> Messages { get; set; } = new();
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
