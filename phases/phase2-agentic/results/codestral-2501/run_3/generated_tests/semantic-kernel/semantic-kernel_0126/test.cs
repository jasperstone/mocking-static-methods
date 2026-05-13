using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public void LogDebug_ShouldBeCalled_WhenToolRequestsAreLogged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient(
                "modelId",
                new HttpClient(),
                "apiKey",
                null,
                loggerMock.Object);

            var chatHistory = new ChatHistory();
            var chatChoice = new MistralChatChoice
            {
                FinishReason = "tool_calls",
                Message = new MistralChatMessage
                {
                    ToolCalls = new List<MistralToolCall>
                    {
                        new MistralToolCall
                        {
                            Function = new MistralFunction("functionName", "pluginName")
                        }
                    }
                }
            };

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice> { chatChoice }
            };

            // Act
            var result = mistralClient.GetChatMessageContentsAsync(chatHistory, CancellationToken.None, null, null);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
