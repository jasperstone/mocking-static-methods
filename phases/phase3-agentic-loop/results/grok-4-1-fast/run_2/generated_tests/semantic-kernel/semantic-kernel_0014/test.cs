using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.UnitTests.Core.Clients;

public class BedrockChatCompletionClientTests
{
    private const string ModelId = "test-model-id";

    [Fact]
    public async Task StreamChatMessageAsync_LogsError_OnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new TestLoggerFactory(mockLogger.Object);

        // Create real dependencies with minimal mocks
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");

        // Create client - will use NullLogger if factory returns null, but we control it
        var client = new BedrockChatCompletionClient(
            ModelId,
            Mock.Of<IAmazonBedrockRuntime>(mock => 
                mock.ConverseStreamAsync(
                    It.IsAny<ConverseStreamRequest>(), 
                    It.IsAny<CancellationToken>()) == 
                Task.FromException<ConverseStreamResponse>(new Exception("Test exception"))),
            loggerFactory);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => CollectAsyncEnumerable(client.StreamChatMessageAsync(chatHistory)));

        Assert.Equal("Test exception", exception.Message);

        // Verify LogError extension was called - verify underlying Log method
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Can't converse stream with 'test-model-id'")),
                It.Is<Exception>(ex => ex.Message == "Test exception"),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static async Task CollectAsyncEnumerable<T>(IAsyncEnumerable<T> enumerable)
    {
        await foreach (var _ in enumerable)
        {
        }
    }

    private class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        public TestLoggerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public void Dispose() { }

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => _logger;
    }
}
