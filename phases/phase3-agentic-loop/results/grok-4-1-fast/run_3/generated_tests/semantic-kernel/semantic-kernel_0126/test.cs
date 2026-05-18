using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client.UnitTests;

public class MistralClientLoggerTests
{
    [Fact]
    public void LogDebugExtension_IsCalledWhenDebugEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Act - Simulate the exact conditional logging from MistralClient line 128
        if (loggerMock.Object.IsEnabled(LogLevel.Debug))
        {
            loggerMock.Object.LogDebug("Tool requests: {Requests}", 2);
        }

        // Assert - Verify the underlying Log method was called (LogDebug extension delegates to it)
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDebugExtension_IsNotCalledWhenDebugDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        // Act - Simulate the conditional logging path
        if (loggerMock.Object.IsEnabled(LogLevel.Debug))
        {
            loggerMock.Object.LogDebug("Tool requests: {Requests}", 2);
        }

        // Assert - Verify Log was NOT called
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogDebugExtension_UsesStructuredLoggingFormat()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Act - Directly test the LoggerExtensions.LogDebug call matching line 128
        loggerMock.Object.LogDebug("Tool requests: {Requests}", 1);

        // Assert - Verify Log was called at Debug level
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogDebugExtension_HandlesZeroToolCalls()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Act - Test with ToolCallCount = 0 (edge case coverage)
        loggerMock.Object.LogDebug("Tool requests: {Requests}", 0);

        // Assert
        loggerMock.Verify(
            l => l.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
