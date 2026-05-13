using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(null, 0, 0, loggerMock.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1 };

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(1, 0, context);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error: 1")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(null, 0, 0, loggerMock.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1 };

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error: 0")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DecrementsCount_WhenCountIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(null, 0, 0, loggerMock.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1 };

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);

            // Assert
            Assert.Equal(0, context.count);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotDecrementCount_WhenCountIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(null, 0, 0, loggerMock.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 0 };

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);

            // Assert
            Assert.Equal(0, context.count);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_ReleasesSemaphore_WhenIssuedFlushIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(null, 0, 0, loggerMock.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1 };
            deltaLog.issuedFlush = 0;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);

            // Assert
            Assert.Equal(1, deltaLog.completedSemaphore.CurrentCount);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotReleaseSemaphore_WhenIssuedFlushIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(null, 0, 0, loggerMock.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1 };
            deltaLog.issuedFlush = 1;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);

            // Assert
            Assert.Equal(0, deltaLog.completedSemaphore.CurrentCount);
        }
    }

    public class PageAsyncFlushResult<T>
    {
        public int count;
        public void Free() { }
    }

    public class Empty { }
}
