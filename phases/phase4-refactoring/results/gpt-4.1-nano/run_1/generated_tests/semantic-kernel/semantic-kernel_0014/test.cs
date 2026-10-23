using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
using Amazon.BedrockRuntime.Model;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;

namespace BedrockClientTests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_Should_LogErrorAndThrow_When_ConverseAsyncThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockService = new Mock<IBedrockChatCompletionService>();
            var mockEndpoint = new Mock<IServiceEndpointProvider>();
            var mockConverseResponse = new ConverseResponse
            {
                Output = new ConverseOutput { Message = new ConverseMessage { Role = new Role { Value = "Assistant" }, Content = new List<ContentBlock> { new ContentBlock { Text = "Hello" } } } },
                Usage = new Usage { InputTokens = 1, OutputTokens = 1 },
                HttpStatusCode = System.Net.HttpStatusCode.OK
            };

            var chatHistory = new ChatHistory();
            var modelId = "model-id";

            // Create the client with the mocked logger
            var client = new BedrockChatCompletionClient(modelId, mockRuntime.Object, new LoggerFactory());

            // Setup the runtime to throw an exception
            mockRuntime.Setup(r => r.DetermineServiceOperationEndpoint(It.IsAny<ConverseRequest>()))
                .Returns(new ServiceEndpoint { URL = "http://endpoint" });
            mockRuntime.Setup(r => r.ConverseAsync(It.IsAny<ConverseRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await client.GenerateChatMessageAsync(chatHistory);
            });

            // Verify that LogError was called with the expected message
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't converse with")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
