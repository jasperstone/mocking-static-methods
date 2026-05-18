using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Generic;
using System;

namespace MistralClientTests
{
    public class MistralClientTests
    {
        [Fact]
        public async Task LogDebug_ShouldBeCalled_WhenToolCallIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MistralClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var chatHistory = new ChatHistory();
            var executionSettings = new PromptExecutionSettings();
            var kernel = new Kernel();

            var mistralClient = new MistralClient("modelId", mockHttpClient.Object, "apiKey", null, mockLogger.Object);

            var chatChoice = new MistralChatChoice
            {
                FinishReason = "tool_calls",
                Message = new MistralChatMessage("assistant", "content")
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
            };

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice> { chatChoice }
            };

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, CancellationToken.None, executionSettings, kernel);

            // Assert
            mockLogger.Verify(
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
