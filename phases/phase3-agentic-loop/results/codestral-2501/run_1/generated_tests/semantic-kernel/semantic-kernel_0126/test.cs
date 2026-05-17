using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;

namespace MistralClientTests
{
    public class MistralClientTests
    {
        [Fact]
        public async Task LogDebug_ShouldBeCalled_WhenToolRequestsExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var chatHistory = new ChatHistory();
            var chatCompletionResponse = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice>
                {
                    new MistralChatChoice
                    {
                        FinishReason = "tool_calls",
                        Message = new MistralChatMessage("assistant", "content")
                        {
                            ToolCalls = new List<MistralToolCall>
                            {
                                new MistralToolCall { Function = new MistralFunction { Name = "TestFunction", Parameters = "param1, param2" } }
                            }
                        }
                    }
                }
            };

            var mistralClient = new MistralClient("modelId", new System.Net.Http.HttpClient(), "apiKey", logger: loggerMock.Object);

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, CancellationToken.None);

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
