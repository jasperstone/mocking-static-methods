using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogWarningExtension_CalledWithException_LogsCorrectly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var exception = new InvalidOperationException("GOSSIP round faulted");
        var message = "GOSSIP round faulted";

        // Act
        mockLogger.Object.LogWarning(exception, message);

        // Assert - Verify the extension method was called with correct parameters
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogWarningExtension_NullLogger_DoesNotThrow()
    {
        // Arrange
        ILogger? logger = null;
        var exception = new InvalidOperationException("test");

        // Act & Assert
        logger?.LogWarning(exception, "GOSSIP round faulted");
    }

    [Fact]
    public void LogWarningExtension_NullLoggerInstance_DoesNotThrow()
    {
        // Arrange
        var logger = NullLogger.Instance;
        var exception = new InvalidOperationException("test");

        // Act & Assert
        logger.LogWarning(exception, "GOSSIP round faulted");
    }
}
