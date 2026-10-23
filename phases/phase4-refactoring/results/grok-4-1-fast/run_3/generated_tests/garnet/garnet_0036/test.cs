using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.tests;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogTrace_ExtensionMethod_CallsUnderlyingLog_WhenTraceEnabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        mockLogger.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 123, "node123");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("[Processed] SetSlot") && 
                    v.ToString()!.Contains("FORCED TO")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogTrace_ExtensionMethod_NullConditional_DoesNotThrow()
    {
        // Arrange
        ILogger logger = null;

        // Act & Assert
        logger?.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 123, "node123");
    }

    [Fact]
    public void LogTrace_ExtensionMethod_SkipsLog_WhenTraceDisabled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        mockLogger.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 123, "node123");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void LogTrace_ExtensionMethod_UsesNullLoggerSafely()
    {
        // Act & Assert
        NullLogger.Instance.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 123, "node123");
    }
}
