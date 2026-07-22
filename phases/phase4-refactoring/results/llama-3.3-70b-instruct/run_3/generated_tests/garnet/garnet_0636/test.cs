using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tsavorite.core;
using Xunit;

namespace TsavoriteTests
{
    public class DeltaLogTests
    {
        [Fact]
        public void LogErrorIsCalledWhenAsyncFlushPageToDeviceCallbackFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 10, 100, loggerMock.Object);

            // Act
            deltaLog.GetType().InvokeMember("AsyncFlushPageToDeviceCallback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, 
                null, deltaLog, new object[] { 1, 0, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void LogErrorIsCalledWhenAsyncReadPagesCallbackFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 10, 100, loggerMock.Object);

            // Act
            deltaLog.GetType().InvokeMember("AsyncReadPagesCallback", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, 
                null, deltaLog, new object[] { 1, 0, null });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
