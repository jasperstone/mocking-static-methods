using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsavorite.core
{
    public class DeltaLogTests
    {
        [Fact]
        public async Task AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 0, 0, loggerMock.Object);

            // Act
            var methodInfo = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(deltaLog, new object[] { (uint)1, (uint)0, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 0, 0, loggerMock.Object);

            // Act
            var methodInfo = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(deltaLog, new object[] { (uint)0, (uint)0, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
