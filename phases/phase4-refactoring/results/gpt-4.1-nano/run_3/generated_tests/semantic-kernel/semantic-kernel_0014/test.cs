using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
using Amazon.BedrockRuntime.Model;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;

namespace BedrockClientTests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_Should_LogError_When_ExceptionThrown()
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
            var client = new BedrockChatCompletionClient("modelId", mockRuntime.Object, null);
            // Setup mocks to throw exception on ConverseAsync
            mockRuntime.Setup(r => r.ConverseAsync(It.IsAny<ConverseRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await client.GenerateChatMessageAsync(chatHistory);
            });

            // Assert
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Can't converse with '{ModelId}'. Reason: {Error}", It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
