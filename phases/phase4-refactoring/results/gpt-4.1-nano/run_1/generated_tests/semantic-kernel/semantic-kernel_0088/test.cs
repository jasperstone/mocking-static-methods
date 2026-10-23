using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading;
using System;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public GeminiChatCompletionClientTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task ProcessFunctionsAsync_ShouldLogDebug_WhenDebugEnabled()
        {
            // Arrange
            var client = new TestGeminiChatCompletionClient(_loggerMock.Object);
            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<ToolCall> { new ToolCall() }
                },
                AutoInvoke = true
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await client.InvokeProcessFunctionsAsync(state, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper classes to simulate the real ones
    internal class TestGeminiChatCompletionClient : GeminiChatCompletionClient
    {
        public TestGeminiChatCompletionClient(ILogger logger) : base(null, "modelId", "apiKey", GoogleAIVersion.V1, logger)
        {
        }

        public async Task InvokeProcessFunctionsAsync(ChatCompletionState state, CancellationToken token)
        {
            await ProcessFunctionsAsync(state, token);
        }

        protected override async Task ProcessFunctionsAsync(ChatCompletionState state, CancellationToken cancellationToken)
        {
            // Call the real method
            await base.ProcessFunctionsAsync(state, cancellationToken);
        }
    }

    // Dummy classes to compile
    internal class ChatCompletionState
    {
        public ChatMessage? LastMessage { get; set; }
        public bool AutoInvoke { get; set; }
        public bool FilterTerminationRequested { get; set; }
    }

    internal class ChatMessage
    {
        public List<ToolCall>? ToolCalls { get; set; }
    }

    internal class ToolCall { }

    internal enum GoogleAIVersion
    {
        V1
    }
}
