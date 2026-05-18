using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests.Gemini.Clients;

public sealed class GeminiChatCompletionClientTests
{
    [Fact]
    public async Task ProcessFunctionsAsync_LogsDebugMessage_WhenDebugEnabledAndToolCallsPresent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        loggerMock.Setup(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyFormat<string>>(state => state.ToString().Contains("Tool requests: 1")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var httpClientMock = new Mock<HttpClient>();
        var fakeState = new { LastMessage = new { ToolCalls = new List<object> { new object(), new object() } } };

        var clientMock = new Mock<GeminiChatCompletionClient>(httpClientMock.Object, "test-model", "test-key", default, loggerMock.Object)
        {
            CallBase = true
        };
        clientMock.Setup(c => c.ProcessFunctionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask)
                  .Verifiable();

        // Act
        await clientMock.Object.ProcessFunctionsAsync(fakeState, CancellationToken.None);

        // Assert
        loggerMock.Verify();
        clientMock.Verify();
    }

    [Fact]
    public async Task ProcessFunctionsAsync_DoesNotLogDebugMessage_WhenDebugDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        var httpClientMock = new Mock<HttpClient>();
        var fakeState = new { LastMessage = new { ToolCalls = new List<object>() } };

        var clientMock = new Mock<GeminiChatCompletionClient>(httpClientMock.Object, "test-model", "test-key", default, loggerMock.Object)
        {
            CallBase = true
        };
        clientMock.Setup(c => c.ProcessFunctionsAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        // Act
        await clientMock.Object.ProcessFunctionsAsync(fakeState, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyFormat<string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
