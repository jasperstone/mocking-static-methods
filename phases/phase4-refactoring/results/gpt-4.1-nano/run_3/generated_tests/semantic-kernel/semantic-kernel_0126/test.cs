using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.MistralAI.Client;

namespace MistralClientTests
{
    public class MistralClientLoggingTests
    {
        [Fact]
        public async Task GetChatMessageContentsAsync_Should_LogDebug_When_ToolCall_IsDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpClient = new HttpClient(); // Could be mocked if needed
            var modelId = "test-model";
            var apiKey = "test-api-key";

            var client = new MistralClient(modelId, httpClient, apiKey, logger: loggerMock.Object);

            // Setup chat history with minimal data
            var chatHistory = new ChatHistory();

            // Setup cancellation token
            var cancellationToken = CancellationToken.None;

            // Act
            await client.GetChatMessageContentsAsync(chatHistory, cancellationToken);

            // Assert
            // Verify that LogDebug was called with a message containing "Tool requests:"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Tool requests:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
