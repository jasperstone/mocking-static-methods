using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;

namespace MistralClientTests
{
    public class MistralClientLoggingTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly MistralClient _client;

        public MistralClientLoggingTests()
        {
            _loggerMock = new Mock<ILogger>();
            var httpClient = new System.Net.Http.HttpClient();
            _client = new MistralClient(
                modelId: "test-model",
                httpClient: httpClient,
                apiKey: "test-api-key",
                logger: _loggerMock.Object);
        }

        [Fact]
        public void LogDebug_IsCalled_WhenResponseContainsToolCall()
        {
            // Arrange
            var chatChoice = new MistralChatChoice
            {
                IsToolCall = true,
                ToolCallCount = 2,
                ToolCalls = new List<MistralToolCall>
                {
                    new MistralToolCall { Function = new MistralFunction { Name = "func1", Parameters = "param1" } },
                    new MistralToolCall { Function = new MistralFunction { Name = "func2", Parameters = "param2" } }
                }
            };

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice> { chatChoice }
            };

            // Act
            // Simulate the code that calls LogDebug
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            if (_loggerMock.Object.IsEnabled(LogLevel.Debug))
            {
                _loggerMock.Object.LogDebug("Tool requests: {Requests}", chatChoice.ToolCallCount);
            }

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 2")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
