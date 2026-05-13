using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Garnet.Tests
{
    public class DeltaLogTests
    {
        [Fact]
        public async Task AsyncFlushPageToDeviceCallback_ErrorLogs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new TestDeltaLog(loggerMock.Object);
            var context = new object();
            uint errorCode = 1; // simulate error
            uint numBytes = 100;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains(nameof(deltaLog.AsyncFlushPageToDeviceCallback))), errorCode),
                Times.Once);
        }

        [Fact]
        public async Task AsyncFlushPageToDeviceCallback_NoError_DoesNotLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deltaLog = new TestDeltaLog(loggerMock.Object);
            var context = new object();
            uint errorCode = 0; // no error
            uint numBytes = 100;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<string>(), It.IsAny<uint>()),
                Times.Never);
        }

        // Additional tests could be added for the decrement logic and semaphore release,
        // but focusing on the LogError call as per the request.

        // Helper class to expose protected method for testing
        private class TestDeltaLog : DeltaLog
        {
            public TestDeltaLog(ILogger logger) : base(logger)
            {
            }

            public new void AsyncFlushPageToDeviceCallback(uint errorCode, uint numBytes, object context)
            {
                base.AsyncFlushPageToDeviceCallback(errorCode, numBytes, context);
            }
        }
    }
}
