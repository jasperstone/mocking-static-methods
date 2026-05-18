using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.UnitTests;

public class BedrockChatCompletionClientTests
{
    private const string ModelId = "test-model";

    [Fact]
    public async Task StreamChatMessageAsync_LogsError_OnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);
        
        var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
        mockBedrockRuntime.Setup(x => x.DetermineServiceOperationEndpoint(It.IsAny<Amazon.BedrockRuntime.Model.ConverseStreamRequest>()))
            .Returns(new Amazon.Runtime.Endpoint { URL = "https://test-endpoint.aws" });
        mockBedrockRuntime.Setup(x => x.ConverseStreamAsync(It.IsAny<Amazon.BedrockRuntime.Model.ConverseStreamRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var client = Activator.CreateInstance(
            typeof(Microsoft.SemanticKernel.Connectors.Amazon.Core.BedrockChatCompletionClient),
            ModelId, mockBedrockRuntime.Object, loggerFactory.Object)!;
        
        // Use reflection to replace the private _logger field
        var loggerField = client.GetType().GetField("_logger", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        loggerField.SetValue(client, mockLogger.Object);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");

        // Act & Assert
        await Assert.ThrowsAnyAsync<InvalidOperationException>(() => 
            CollectAsyncEnumerable((dynamic)client.StreamChatMessageAsync(chatHistory)));

        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "Can't converse stream with '{ModelId}'. Reason: {Error}",
                ModelId,
                "Test exception"),
            Times.Once);
    }

    [Fact]
    public async Task GenerateChatMessageAsync_LogsError_OnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(mockLogger.Object);
        
        var mockBedrockRuntime = new Mock<IAmazonBedrockRuntime>();
        mockBedrockRuntime.Setup(x => x.DetermineServiceOperationEndpoint(It.IsAny<Amazon.BedrockRuntime.Model.ConverseRequest>()))
            .Returns(new Amazon.Runtime.Endpoint { URL = "https://test-endpoint.aws" });
        mockBedrockRuntime.Setup(x => x.ConverseAsync(It.IsAny<Amazon.BedrockRuntime.Model.ConverseRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var client = Activator.CreateInstance(
            typeof(Microsoft.SemanticKernel.Connectors.Amazon.Core.BedrockChatCompletionClient),
            ModelId, mockBedrockRuntime.Object, loggerFactory.Object)!;
        
        // Use reflection to replace the private _logger field
        var loggerField = client.GetType().GetField("_logger", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        loggerField.SetValue(client, mockLogger.Object);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("test");

        // Act & Assert
        await Assert.ThrowsAnyAsync<InvalidOperationException>(() => 
            ((dynamic)client).GenerateChatMessageAsync(chatHistory));

        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "Can't converse with '{ModelId}'. Reason: {Error}",
                ModelId,
                "Test exception"),
            Times.Once);
    }

    private static async Task CollectAsyncEnumerable<T>(IAsyncEnumerable<T> enumerable)
    {
        await foreach (var _ in enumerable)
        {
        }
    }
}
