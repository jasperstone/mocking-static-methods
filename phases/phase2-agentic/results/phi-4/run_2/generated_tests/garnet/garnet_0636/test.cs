using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(
                new Mock<IDevice>().Object,
                12, // Example log page size bits
                0,
                loggerMock.Object);

            uint errorCode = 1; // Non-zero error code
            object context = new PageAsyncFlushResult<Empty> { count = 1 };

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(errorCode, 0, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("AsyncFlushPageToDeviceCallback error:")),
                    errorCode),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new DeltaLog(
                new Mock<IDevice>().Object,
                12, // Example log page size bits
                0,
                loggerMock.Object);

            uint errorCode = 0; // Zero error code
            object context = new PageAsyncFlushResult<Empty> { count = 1 };

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(errorCode, 0, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<uint>()),
                Times.Never);
        }
    }
}
