using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using Xunit;

public class GeminiChatCompletionClientTests
{
    [Fact]
    public async Task LogDebug_CalledWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GeminiChatCompletionClient>>();
        var client = new GeminiChatCompletionClient(
            new System.Net.Http.HttpClient(),
            "modelId",
            "apiKey",
            GoogleAIVersion.V1,
            mockLogger.Object);

        var chatHistory = new ChatHistory();
        var executionSettings = new PromptExecutionSettings();
        var kernel = new Kernel();

        // Act
        await client.GenerateChatMessageAsync(chatHistory, executionSettings, kernel, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Once);
    }
}
