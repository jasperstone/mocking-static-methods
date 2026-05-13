using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.Google.Core;

public class GeminiChatCompletionClientTests
{
    [Fact]
    public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenToolRequestsArePresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var client = new GeminiChatCompletionClient(
            new HttpClient(),
            "modelId",
            "apiKey",
            GoogleAIVersion.V1,
            loggerMock.Object);

        var state = new ChatCompletionState
        {
            LastMessage = new ChatMessage
            {
                ToolCalls = new[] { new ToolCall() }
            }
        };

        // Act
        await client.ProcessFunctionsAsync(state, CancellationToken.None);

        // Assert
        loggerMock.Verify(l => l.LogDebug("Tool requests: {Requests}", 1), Times.Once);
    }

    [Fact]
    public async Task ProcessFunctionsAsync_LogsTraceMessage_WhenToolRequestsArePresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
        var client = new GeminiChatCompletionClient(
            new HttpClient(),
            "modelId",
            "apiKey",
            GoogleAIVersion.V1,
            loggerMock.Object);

        var state = new ChatCompletionState
        {
            LastMessage = new ChatMessage
            {
                ToolCalls = new[] { new ToolCall() }
            }
        };

        // Act
        await client.ProcessFunctionsAsync(state, CancellationToken.None);

        // Assert
        loggerMock.Verify(l => l.LogTrace("Function call requests: {FunctionCall}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ProcessFunctionsAsync_DoesNotLogDebugMessage_WhenToolRequestsAreNotPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var client = new GeminiChatCompletionClient(
            new HttpClient(),
            "modelId",
            "apiKey",
            GoogleAIVersion.V1,
            loggerMock.Object);

        var state = new ChatCompletionState
        {
            LastMessage = new ChatMessage
            {
                ToolCalls = null
            }
        };

        // Act
        await client.ProcessFunctionsAsync(state, CancellationToken.None);

        // Assert
        loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
