using System;
using System.Collections.Generic;
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
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolCallCountIsLogged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mistralClient = new MistralClient("modelId", new System.Net.Http.HttpClient(), "apiKey", logger: loggerMock.Object);

            var chatHistory = new ChatHistory();
            var cancellationToken = new CancellationToken();
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice>
                {
                    new MistralChatChoice
                    {
                        IsToolCall = true,
                        ToolCallCount = 1,
                        ToolCalls = new List<ToolCall>
                        {
                            new ToolCall
                            {
                                Function = new Function
                                {
                                    Name = "TestFunction",
                                    Parameters = new Dictionary<string, object>()
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
            loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
