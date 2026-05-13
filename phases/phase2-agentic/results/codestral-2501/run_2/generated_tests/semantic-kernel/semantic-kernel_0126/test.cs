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
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolCallIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClientMock = new Mock<HttpClient>();
            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();

            var mistralClient = new MistralClient(
                "modelId",
                httpClientMock.Object,
                "apiKey",
                null,
                loggerMock.Object);

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice>
                {
                    new MistralChatChoice
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
                    }
                }
            };

            loggerMock.Setup(logger => logger.IsEnabled(LogLevel.Debug)).Returns(true);

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, cancellationToken, executionSettings, kernel);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
