using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Cluster.Tests;

public class LoggerExtensionsTests
{
    [Fact]
    public void IOCallback_NoError_DoesNotLogError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = mockLogger.Object;
        var context = new SemaphoreSlim(0, 1);

        // Act
        ((ILogger)logger).IOCallback(0, 123, context);

        // Assert
        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
        
        Assert.Equal(1, context.CurrentCount);
    }

    [Fact]
    public void IOCallback_WithError_LogsErrorMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = mockLogger.Object;
        var context = new SemaphoreSlim(0, 1);

        // Act
        ((ILogger)logger).IOCallback(5, 0, context);

        // Assert
        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>(state => state.ToString().Contains("[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: 5")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        
        Assert.Equal(1, context.CurrentCount);
    }

    [Fact]
    public void IOCallback_NullLogger_DoesNotThrow()
    {
        // Arrange
        ILogger? logger = null;
        var context = new SemaphoreSlim(0, 1);
        
        // Act
        logger?.IOCallback(5, 0, context);
        
        // Assert
        Assert.Equal(1, context.CurrentCount);
    }

    [Fact]
    public void IOCallback_AlwaysReleasesSemaphore()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = mockLogger.Object;
        var context = new SemaphoreSlim(0, 1);

        // Act - no error case
        ((ILogger)logger).IOCallback(0, 0, context);
        Assert.Equal(1, context.CurrentCount);
        
        // Reset for error case
        context = new SemaphoreSlim(0, 1);
        ((ILogger)logger).IOCallback(123, 456, context);
        Assert.Equal(1, context.CurrentCount);
    }
}
