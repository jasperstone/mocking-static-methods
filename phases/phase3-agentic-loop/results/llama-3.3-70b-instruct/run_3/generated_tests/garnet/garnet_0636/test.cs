using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public async Task FlushAsync_LogsErrorOnAsyncFlushPageToDeviceCallbackError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 10, 0, loggerMock.Object);

            // Act
            var pageAsyncFlushResult = new PageAsyncFlushResult<Empty>();
            pageAsyncFlushResult.count = 1;
            var type = typeof(DeltaLog);
            var methodInfo = type.GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(deltaLog, new object[] { 1, 0, pageAsyncFlushResult });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task FlushAsync_CompletesSuccessfullyWhenNoErrorsOccur()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLogDeviceMock = new Mock<IDevice>();
            var deltaLog = new DeltaLog(deltaLogDeviceMock.Object, 10, 0, loggerMock.Object);

            // Act
            await deltaLog.FlushAsync();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
