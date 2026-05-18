using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private Mock<ILogger> CreateLoggerMock(bool isEnabledDebug = true, bool isEnabledTrace = false)
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(isEnabledDebug);
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(isEnabledTrace);
            return mockLogger;
        }

        [Fact]
        public async Task ProcessFunctionsAsync_LogsDebug_WhenDebugEnabled()
        {
            // Arrange
            var mockLogger = CreateLoggerMock(isEnabledDebug: true);
            var client = new GeminiChatCompletionClientMock(mockLogger.Object);
            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<string> { "call1", "call2" }
                },
                AutoInvoke = true
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await client.ProcessFunctionsAsync(state, cancellationToken);

            // Assert
            mockLogger.Verify(l => l.LogDebug("Tool requests: {Requests}", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task ProcessFunctionsAsync_DoesNotLogDebug_WhenDebugDisabled()
        {
            // Arrange
            var mockLogger = CreateLoggerMock(isEnabledDebug: false);
            var client = new GeminiChatCompletionClientMock(mockLogger.Object);
            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<string> { "call1" }
                },
                AutoInvoke = true
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await client.ProcessFunctionsAsync(state, cancellationToken);

            // Assert
            mockLogger.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }

    // Minimal mock class to test the method
    internal class GeminiChatCompletionClientMock : GeminiChatCompletionClient
    {
        public GeminiChatCompletionClientMock(ILogger logger) : base(new HttpClient(), "modelId", "apiKey", GoogleAIVersion.V1, logger)
        {
        }

        public new async Task ProcessFunctionsAsync(ChatCompletionState state, CancellationToken cancellationToken)
        {
            await base.ProcessFunctionsAsync(state, cancellationToken);
        }
    }

    // Dummy classes to simulate the real ones
    public class ChatCompletionState
    {
        public ChatMessage? LastMessage { get; set; }
        public bool AutoInvoke { get; set; }
        public bool FilterTerminationRequested { get; set; }
    }

    public class ChatMessage
    {
        public List<string>? ToolCalls { get; set; }
    }

    public enum GoogleAIVersion
    {
        V1
    }
}
