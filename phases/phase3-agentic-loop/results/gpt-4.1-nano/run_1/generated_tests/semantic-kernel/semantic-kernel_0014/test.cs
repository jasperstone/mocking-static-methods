using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

namespace Microsoft.SemanticKernel.Tests
{
    public class BedrockChatCompletionClientTests
    {
        [Fact]
        public async Task GenerateChatMessageAsync_Should_LogError_When_ConverseAsync_Throws()
        {
            // Arrange
            var mockRuntime = new Mock<IAmazonBedrockRuntime>();
            var mockLogger = new Mock<ILogger>();
            var mockConverseRequest = new ConverseRequest();

            // Setup the runtime to return a fixed URL
            mockRuntime.Setup(r => r.DetermineServiceOperationEndpoint(It.IsAny<ConverseRequest>()))
                .Returns(new ServiceEndpoint { URL = "https://testendpoint" });

            // Setup ConverseAsync to throw
            mockRuntime.Setup(r => r.ConverseAsync(It.IsAny<ConverseRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Create a mock for the logger factory
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<BedrockChatCompletionClient>();
            var mockLoggerProvider = new Mock<ILoggerProvider>();
            mockLoggerProvider.Setup(p => p.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            loggerFactory.AddProvider(mockLoggerProvider.Object);

            // Instantiate the client with the mock runtime and logger
            var client = new BedrockChatCompletionClient("testModelId", mockRuntime.Object, loggerFactory);

            var chatHistory = new ChatHistory();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await client.GenerateChatMessageAsync(chatHistory);
            });

            // Verify that LogError was called
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
