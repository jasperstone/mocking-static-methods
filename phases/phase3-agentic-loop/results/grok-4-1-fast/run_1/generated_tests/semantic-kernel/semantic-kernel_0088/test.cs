using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.UnitTests.Gemini.Clients;

public sealed class LoggerExtensionsTests
{
    [Fact]
    public void LogDebug_ToolRequests_CallsLoggerLogWithCorrectFormat()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var count = 3;
        var state = new { LastMessage = new { ToolCalls = new List<object> { new(), new(), new() } } };
        
        // Act
        loggerMock.Object.LogDebug("Tool requests: {Requests}", count);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tool requests: 3")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDebug_ToolRequests_DoesNotLog_WhenDebugDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
        
        var count = 3;

        // Act
        loggerMock.Object.LogDebug("Tool requests: {Requests}", count);

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
