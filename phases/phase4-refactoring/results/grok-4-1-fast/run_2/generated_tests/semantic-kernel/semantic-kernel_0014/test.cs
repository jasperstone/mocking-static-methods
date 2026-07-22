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
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.UnitTests;

public class BedrockChatCompletionClientLoggerTests
{
    private const string ModelId = "test-model-id";

    [Fact]
    public async Task StreamChatMessageAsync_LogsError_OnConverseStreamAsyncException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v?.ToString()?.Contains("Can't converse stream with") == true &&
                v?.ToString()?.Contains(ModelId) == true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var loggerFactory = new TestLoggerFactory(mockLogger.Object);
        
        // Since BedrockChatCompletionClient is internal, we use reflection to create instance
        // This tests the LogError call without needing direct access to internal types
        var clientType = Type.GetType("Microsoft.SemanticKernel.Connectors.Amazon.Core.BedrockChatCompletionClient, Microsoft.SemanticKernel.Connectors.Amazon.Bedrock")!;
        var client = Activator.CreateInstance(clientType, ModelId, null!, loggerFactory)!;

        var streamMethod = clientType.GetMethod("StreamChatMessageAsync", 
            new[] { typeof(ChatHistory), typeof(PromptExecutionSettings), typeof(Kernel), typeof(CancellationToken) })!;
        
        // Act & Assert
        var ex = await Assert.ThrowsAnyAsync<Exception>(
            () => RunAsyncEnumerable((IAsyncEnumerable<object>)streamMethod.Invoke(client, new object[] { new ChatHistory(), null, null, default })!));

        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static async Task RunAsyncEnumerable(IAsyncEnumerable<object> enumerable)
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
