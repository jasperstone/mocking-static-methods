using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using Xunit;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_LogsErrorOnException()
        {
            // Arrange
            var modelId = "test-model";
            var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var mockChatService = new Mock<IBedrockChatCompletionService>();
            var chatHistory = new ChatHistory("user");
            var executionSettings = new PromptExecutionSettings();

            // Setup _ioChatService.GetConverseRequest to return a dummy ConverseRequest
            var converseRequest = new ConverseRequest();
            mockChatService.Setup(s => s.GetConverseRequest(modelId, chatHistory, executionSettings)).Returns(converseRequest);

            // Setup _bedrockRuntime.DetermineServiceOperationEndpoint to return a dummy endpoint
            var endpoint = new Amazon.BedrockRuntime.Model.Endpoint { URL = "https://dummy.endpoint" };
            mockBedrockRuntime.Setup(r => r.DetermineServiceOperationEndpoint(converseRequest)).Returns(endpoint);

            // Setup _bedrockRuntime.ConverseAsync to throw an exception to trigger the catch block
            var testException = new InvalidOperationException("Test exception");
            mockBedrockRuntime.Setup(r => r.ConverseAsync(converseRequest, It.IsAny<CancellationToken>())).ThrowsAsync(testException);

            // Create instance of BedrockChatCompletionClient with mocks
            var client = new BedrockChatCompletionClient(modelId, mockBedrockRuntime.Object, mockLoggerFactory.Object);

            // Use reflection to replace the private _ioChatService field with our mockChatService
            var ioChatServiceField = typeof(BedrockChatCompletionClient).GetField("_ioChatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ioChatServiceField.SetValue(client, mockChatService.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await client.GenerateChatMessageAsync(chatHistory, executionSettings);
            });

            // Verify that LogError was called with the expected parameters
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't converse with")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
