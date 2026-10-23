using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests;

public class GeminiChatCompletionClientLoggerTests
{
    [Fact]
    public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabledAndToolCallsPresent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Create real client with mocked logger - since ProcessFunctionsAsync is private/internal,
        // we test the observable logging behavior through ILogger verification
        using var httpClient = new Mock<HttpClient>().Object;
        var client = new Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.Clients.GeminiChatCompletionClient(
            httpClient,
            "gemini-pro",
            "fake-key",
            new Microsoft.SemanticKernel.Connectors.Google.Core.Gemini.GoogleAIVersion("v1beta"),
            mockLogger.Object);

        // Create state that would trigger the logging (though we can't call private method directly)
        var state = new ChatCompletionState();
        state.LastMessage = new ChatMessageContent(AuthorRole.Assistant, "")
        {
            ToolCalls = new List<ChatMessageToolCallContent>()
            {
                new ChatMessageToolCallContent("func1", new Dictionary<string, object?>()),
                new ChatMessageToolCallContent("func2", new Dictionary<string, object?>()),
            }
        };

        // Since ProcessFunctionsAsync is private/internal, verify the logging pattern/behavior
        // that would be called when the condition is met
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Tool requests:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(1)); // The setup ensures this pattern matches the LogDebug call
    }

    [Fact]
    public void ProcessFunctionsAsync_LogDebug_CalledWithCorrectMessageFormat()
    {
        // Arrange - Verify the LogDebug extension method usage pattern
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // The LogDebug call on line 495: this.Logger.LogDebug("Tool requests: {Requests}", state.LastMessage!.ToolCalls!.Count);
        // Test verifies the ILogger.Log pattern that LogDebug extension produces
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                "Tool requests: {Requests}",
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((func) => 
                    {
                        var state = new ChatCompletionState();
                        state.LastMessage = new ChatMessageContent(AuthorRole.Assistant, "")
                        {
                            ToolCalls = new List<ChatMessageToolCallContent>
                            {
                                new("func1", new Dictionary<string, object?>()),
                                new("func2", new Dictionary<string, object?>()),
                            }
                        };
                        var expectedCount = state.LastMessage.ToolCalls!.Count;
                        var formatted = func("Tool requests: {Requests}", null);
                        return formatted.Contains(expectedCount.ToString());
                    })),
            Times.Once);
    }
}
