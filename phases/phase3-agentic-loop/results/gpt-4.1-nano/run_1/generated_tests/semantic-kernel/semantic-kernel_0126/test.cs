using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task LogDebug_IsCalled_WhenResponseContainsToolCall()
        {
            // Arrange
            var chatChoice = new MistralChatChoice
            {
                IsToolCall = true,
                ToolCallCount = 1,
                ToolCalls = new List<MistralToolCall>
                {
                    new MistralToolCall
                    {
                        Function = new MistralFunction { Name = "TestFunction", Parameters = "{}" }
                    }
                }
            };

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice> { chatChoice }
            };

            // Use reflection to invoke the internal method that contains the logging logic
            var methodInfo = typeof(MistralClient).GetMethod("ProcessResponseAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            var chatHistory = new List<ChatMessageContent>();
            var cancellationToken = new CancellationToken();

            // Act
            await (Task)methodInfo.Invoke(_client, new object[] { responseData, chatHistory, null, null, cancellationToken });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
