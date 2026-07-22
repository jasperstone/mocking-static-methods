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
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolCallIsPresent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MistralClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var chatHistory = new ChatHistory();
            var cancellationToken = CancellationToken.None;
            var mistralClient = new MistralClient("modelId", mockHttpClient.Object, "apiKey", logger: mockLogger.Object);

            var chatChoice = new MistralChatChoice
            {
                IsToolCall = true,
                ToolCallCount = 1,
                ToolCalls = new List<MistralToolCall>
                {
                    new MistralToolCall
                    {
                        Function = new MistralFunction
                        {
                            Name = "TestFunction",
                            Parameters = new Dictionary<string, object>()
                        }
                    }
                }
            };

            var responseData = new ChatCompletionResponse
            {
                Choices = new List<MistralChatChoice> { chatChoice }
            };

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, cancellationToken);

            // Assert
            mockLogger.Verify(
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
