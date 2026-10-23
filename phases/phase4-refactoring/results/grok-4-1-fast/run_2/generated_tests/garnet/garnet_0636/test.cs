using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeltaLog>>();

            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(d => d.SectorSize).Returns(512UL);
            mockDevice.Setup(d => d.GetFileSize(It.IsAny<int>())).Returns(0L);

            var deltaLog = new DeltaLog(mockDevice.Object, 12, 0, loggerMock.Object);

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = new PageAsyncFlushResult<Empty> { count = 1 };
            var errorCode = 100u;

            // Act
            try
            {
                method!.Invoke(deltaLog, new object[] { errorCode, 0u, result });
            }
            catch
            {
                // Ignore any exceptions (disposed, etc.)
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(msg => msg.Contains("AsyncFlushPageToDeviceCallback error") && msg.Contains("{errorCode}")),
                    errorCode
                ),
                Times.Once
            );
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DeltaLog>>();

            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(d => d.SectorSize).Returns(512UL);
            mockDevice.Setup(d => d.GetFileSize(It.IsAny<int>())).Returns(0L);

            var deltaLog = new DeltaLog(mockDevice.Object, 12, 0, loggerMock.Object);

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = new PageAsyncFlushResult<Empty> { count = 1 };

            // Act
            try
            {
                method!.Invoke(deltaLog, new object[] { 0u, 1024u, result });
            }
            catch
            {
                // Ignore any exceptions
            }

            // Assert - no LogError call
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<uint>()), Times.Never);
        }
    }
}
