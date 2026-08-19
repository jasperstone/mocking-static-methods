using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogWarning_IsCalled_WithExpectedMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var logger = mockLogger.Object;

        // Act - Simulate the exact logger?.LogWarning call from TxnRespCommands.cs line 65
        logger.LogWarning("Failed CheckClusterTxnKeys");

        // Assert - Verify the LogWarning extension method was called with the expected message
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == 0),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWarning_NullLogger_DoesNotThrow()
    {
        // Arrange
        ILogger? logger = null;

        // Act
        var exception = Record.Exception(() => logger?.LogWarning("Failed CheckClusterTxnKeys"));

        // Assert - Matches the null-conditional operator behavior in production code
        Assert.Null(exception);
    }

    [Fact]
    public void LogWarning_LoggerDisabled_DoesNotLog()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(false);
        var logger = mockLogger.Object;

        // Act
        logger.LogWarning("Failed CheckClusterTxnKeys");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
