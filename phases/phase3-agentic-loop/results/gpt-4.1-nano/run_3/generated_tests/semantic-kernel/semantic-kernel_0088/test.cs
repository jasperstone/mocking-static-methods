using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<HttpClient> _httpClientMock;

        public GeminiChatCompletionClientTests()
        {
            _loggerMock = new Mock<ILogger>();
            _httpClientMock = new Mock<HttpClient>();
        }

        [Fact]
        public void ProcessFunctionsAsync_LogsDebug_WhenDebugEnabledAndToolCallsExist()
        {
            // Arrange
            var client = new GeminiChatCompletionClient(
                _httpClientMock.Object,
                "modelId",
                "apiKey",
                GoogleAIVersion.V1,
                _loggerMock.Object);

            var state = new ChatCompletionState
            {
                LastMessage = new ChatMessage
                {
                    ToolCalls = new List<string> { "call1", "call2" }
                },
                AutoInvoke = true,
                FilterTerminationRequested = false
            };

            // Act
            var task = client.ProcessFunctionsAsync(state, CancellationToken.None);
            task.Wait();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 2")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
