using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.Tests
{
    public class MistralClientTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MistralClient>>();
            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();

            var mistralClient = new MistralClient(
                "modelId",
                new HttpClient(),
                "apiKey",
                new Uri("https://api.mistral.ai"),
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
                                    Function = new MistralFunction
                                    {
                                        Name = "TestFunction",
                                        Parameters = "param1, param2"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, cancellationToken, executionSettings, kernel);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests: 1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
