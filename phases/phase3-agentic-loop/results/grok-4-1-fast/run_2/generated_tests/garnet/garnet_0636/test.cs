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
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(d => d.SectorSize).Returns((uint)512);
            mockDevice.Setup(d => d.GetFileSize(It.IsAny<int>())).Returns(0L);

            var deltaLog = new DeltaLog(mockDevice.Object, 12, 0, loggerMock.Object);
            
            uint errorCode = 123;
            uint numBytes = 0;
            var context = new PageAsyncFlushResult<Empty>() { count = 1 };

            // Act
            deltaLog.GetType()
                .GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(deltaLog, new object[] { errorCode, numBytes, context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(d => d.SectorSize).Returns((uint)512);
            mockDevice.Setup(d => d.GetFileSize(It.IsAny<int>())).Returns(0L);

            var deltaLog = new DeltaLog(mockDevice.Object, 12, 0, loggerMock.Object);
            
            uint errorCode = 0;
            uint numBytes = 1024;
            var context = new PageAsyncFlushResult<Empty>() { count = 1 };

            // Act
            deltaLog.GetType()
                .GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(deltaLog, new object[] { errorCode, numBytes, context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
