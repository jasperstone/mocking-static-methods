using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.UnitTests;

public class GeminiChatCompletionClientTests
{
    private sealed class TestGeminiChatCompletionClient : GeminiChatCompletionClient
    {
        public TestGeminiChatCompletionClient(HttpClient httpClient, string modelId, string apiKey, GoogleAIVersion apiVersion, ILogger logger)
            : base(httpClient, modelId, apiKey, apiVersion, logger)
        {
        }

        public async Task TestProcessFunctionsAsync(ChatCompletionState state, CancellationToken cancellationToken = default)
        {
            await ProcessFunctionsAsync(state, cancellationToken).ConfigureAwait(false);
        }
    }

    [Fact]
    public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabledAndToolCallsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var logger = loggerMock.Object;

        var httpClient = new Mock<HttpClient>().Object;
        var client = new TestGeminiChatCompletionClient(httpClient, "test-model", "test-api-key", GoogleAIVersion.V1beta, logger);

        var state = new ChatCompletionState();
        var lastMessage = new ChatMessageContent(AuthorRole.Assistant, "");
        lastMessage.ToolCalls = new List<ToolCallContent> { new ToolCallContent("", new AuthorRoleConverter().FromAuthorRole(AuthorRole.Assistant), new List<ChatMessageContent>()) };
        state.LastMessage = lastMessage;

        // Act
        await client.TestProcessFunctionsAsync(state);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(o => ((string)o.ToString()).Contains("Tool requests: 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessFunctionsAsync_DoesNotLogDebug_WhenDebugDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GeminiChatCompletionClient>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        var logger = loggerMock.Object;

        var httpClient = new Mock<HttpClient>().Object;
        var client = new TestGeminiChatCompletionClient(httpClient, "test-model", "test-api-key", GoogleAIVersion.V1beta, logger);

        var state = new ChatCompletionState();
        var lastMessage = new ChatMessageContent(AuthorRole.Assistant, "");
        lastMessage.ToolCalls = new List<ToolCallContent> { new() };
        state.LastMessage = lastMessage;

        // Act
        await client.TestProcessFunctionsAsync(state);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
