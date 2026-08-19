using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google.Core;

public class GeminiChatCompletionClientTests
{
    [Fact]
    public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenToolCallsArePresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var client = new Microsoft.SemanticKernel.Connectors.Google.Core.GeminiChatCompletionClient(
            new HttpClient(),
            "modelId",
            "apiKey",
            Microsoft.SemanticKernel.Connectors.Google.Core.GoogleAIVersion.V1,
            loggerMock.Object);

        var state = new Microsoft.SemanticKernel.ChatCompletion.ChatCompletionState();
        state.LastMessage = new Microsoft.SemanticKernel.ChatCompletion.ChatMessage();
        state.LastMessage.ToolCalls = new System.Collections.Generic.List<Microsoft.SemanticKernel.ChatCompletion.ToolCall> { new Microsoft.SemanticKernel.ChatCompletion.ToolCall() };

        // Act
        await client.ProcessFunctionsAsync(state, CancellationToken.None);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", 1), Times.Once);
    }
}
