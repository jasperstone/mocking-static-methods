using System;
using System.Collections.Generic;
using System.Net.Http;
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
        public async Task GetChatMessageContentsAsync_LogsDebug_WhenToolRequestsArePresent()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MistralClient>>();
            var mockHttpClient = new Mock<HttpClient>();
            var chatHistory = new ChatHistory();
            var cancellationToken = new CancellationToken();
            var mistralClient = new MistralClient("modelId", mockHttpClient.Object, "apiKey", logger: mockLogger.Object);

            // Mock the HTTP response
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
                                    Parameters = "param1, param2"
                                }
                            }
                        }
                    }
                }
            };

            mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(responseData))
                });

            // Act
            await mistralClient.GetChatMessageContentsAsync(chatHistory, cancellationToken);

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
