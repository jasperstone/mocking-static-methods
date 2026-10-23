using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BedrockClientTests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_LogsErrorAndThrows_WhenConverseAsyncThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockService = new Mock<IBedrockChatCompletionService>();
            var mockEndpoint = new Mock<IServiceEndpointProvider>();
            var mockDiagnostics = new Mock<IModelDiagnostics>();

            var chatHistory = new ChatHistory();
            var modelId = "test-model";

            // Setup mock runtime to throw
            mockRuntime.Setup(r => r.ConverseAsync(It.IsAny<ConverseRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Instantiate client with mocks
            var client = new BedrockChatCompletionClient(modelId, mockRuntime.Object, null);
            // Inject mocks
            var clientType = typeof(BedrockChatCompletionClient);
            var clientInstance = (dynamic)client;
            clientInstance._logger = mockLogger.Object;
            clientInstance._ioChatService = mockService.Object;
            clientInstance._modelProvider = "test-provider";
            clientInstance._bedrockRuntime = mockRuntime.Object;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await client.GenerateChatMessageAsync(chatHistory);
            });

            // Verify LogError was called
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
