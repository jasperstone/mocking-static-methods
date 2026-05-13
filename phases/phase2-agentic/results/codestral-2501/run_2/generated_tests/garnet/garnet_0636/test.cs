using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

public class DeltaLogTests
{
    [Fact]
    public async Task FlushAsync_ShouldFlushPageAndReleaseSemaphore()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(null, 12, 0, mockLogger.Object);

        // Act
        await deltaLog.FlushAsync();

        // Assert
        // Verify that FlushPage is called if needed
        // Verify that completedSemaphore is released and waited on
        // Verify that issuedFlush is incremented
        Assert.Equal(1, deltaLog.issuedFlush);
        Assert.Equal(0, deltaLog.completedSemaphore.CurrentCount);
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_ShouldLogErrorOnNonZeroErrorCode()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(null, 12, 0, mockLogger.Object);
        var errorCode = 1u;
        var numBytes = 0u;
        var context = new PageAsyncFlushResult<Empty> { count = 1 };

        // Act
        deltaLog.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error: 1")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_ShouldDecrementCountAndFreeResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(null, 12, 0, mockLogger.Object);
        var errorCode = 0u;
        var numBytes = 0u;
        var context = new PageAsyncFlushResult<Empty> { count = 1 };

        // Act
        deltaLog.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);

        // Assert
        Assert.Equal(0, context.count);
        Assert.True(context.IsFreed);
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_ShouldReleaseSemaphoreWhenIssuedFlushIsZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var deltaLog = new DeltaLog(null, 12, 0, mockLogger.Object);
        var errorCode = 0u;
        var numBytes = 0u;
        var context = new PageAsyncFlushResult<Empty> { count = 1 };
        deltaLog.issuedFlush = 0;

        // Act
        deltaLog.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);

        // Assert
        Assert.Equal(1, deltaLog.completedSemaphore.CurrentCount);
    }
}

public class PageAsyncFlushResult<T>
{
    public int count;
    public bool IsFreed { get; private set; }

    public void Free()
    {
        IsFreed = true;
    }
}

public class Empty { }
