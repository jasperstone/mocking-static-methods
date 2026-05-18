using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq.Expressions;

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
            mockDevice.Setup(x => x.SectorSize).Returns(512u);
            mockDevice.Setup(x => x.GetFileSize(It.IsAny<int>())).Returns(0L);

            var deltaLog = new DeltaLog(mockDevice.Object, 12, 0, loggerMock.Object);

            // Capture logger calls using Strict mock
            loggerMock.Setup(x => x.Log(
                It.Is<LogEntry>(e => e.LogLevel == LogLevel.Error),
                It.IsAny<Exception>()))
                .Callback<LogEntry, Exception>((entry, ex) => {
                    Assert.Contains("AsyncFlushPageToDeviceCallback error:", entry.Formatter(entry.State, ex));
                    Assert.Contains("123", entry.Formatter(entry.State, ex));
                });

            uint errorCode = 123;
            uint numBytes = 0;
            
            // Create a real PageAsyncFlushResult<Empty> instance
            var result = new PageAsyncFlushResult<Empty>();
            // Set up the fields it expects using reflection since it's likely private/internal
            var countField = typeof(PageAsyncFlushResult<Empty>).GetField("count", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countField?.SetValue(result, 1);

            // Act
            // Use reflection to invoke the private method
            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(deltaLog, new object[] { errorCode, numBytes, result });

            // Assert
            loggerMock.Verify(x => x.Log(
                It.Is<LogEntry>(e => e.LogLevel == LogLevel.Error),
                It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(x => x.SectorSize).Returns(512u);
            mockDevice.Setup(x => x.GetFileSize(It.IsAny<int>())).Returns(0L);

            var deltaLog = new DeltaLog(mockDevice.Object, 12, 0, loggerMock.Object);

            uint errorCode = 0;
            uint numBytes = 1024;
            
            var result = new PageAsyncFlushResult<Empty>();
            var countField = typeof(PageAsyncFlushResult<Empty>).GetField("count", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countField?.SetValue(result, 1);

            // Act
            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(deltaLog, new object[] { errorCode, numBytes, result });

            // Assert
            loggerMock.Verify(x => x.Log(
                It.Is<LogEntry>(e => e.LogLevel == LogLevel.Error),
                It.IsAny<Exception>()), Times.Never);
        }
    }
}
