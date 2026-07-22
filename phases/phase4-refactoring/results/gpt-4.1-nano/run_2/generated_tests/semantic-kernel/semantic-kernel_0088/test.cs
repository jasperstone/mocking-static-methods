using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google.Core;

namespace SemanticKernel.Tests
{
    public class GeminiChatCompletionClientTests
    {
        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenLogLevelEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            var client = new TestGeminiChatCompletionClient(loggerMock.Object);

            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<ToolCall>
                    {
                        new ToolCall { Name = "TestTool" }
                    }
                },
                AutoInvoke = true,
                FilterTerminationRequested = false
            };

            var cancellationToken = CancellationToken.None;

            // Act
            await client.InvokeProcessFunctionsAsync(state, cancellationToken);

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests:")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }

    // A test subclass to access the private method
    internal class TestGeminiChatCompletionClient : GeminiChatCompletionClient
    {
        public TestGeminiChatCompletionClient(ILogger logger) : base(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, logger)
        {
        }

        public async Task InvokeProcessFunctionsAsync(ChatCompletionState state, CancellationToken token)
        {
            await this.ProcessFunctionsAsync(state, token);
        }
    }
}
