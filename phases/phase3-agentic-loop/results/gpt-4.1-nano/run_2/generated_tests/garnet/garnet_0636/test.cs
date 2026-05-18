using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class DeltaLogTests
    {
        private class DummyDevice : IDevice
        {
            public long GetFileSize(int index) => 1024;
            public ulong SectorSize => 512;
            public void ReadAsync(ulong offset, IntPtr buffer, uint length, Action<uint, uint, object> callback, object state)
            {
                // Simulate successful read
                callback(0, length, state);
            }
        }

        private class DummyPageAsyncFlushResult : PageAsyncFlushResult<Empty>
        {
            public int count;
            public void Free() { }
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);
            deltaLog.completedSemaphore = new SemaphoreSlim(0);
            deltaLog.issuedFlush = 1;

            var result = new DummyPageAsyncFlushResult { count = 1 };
            var context = new object();

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(1, 0, context);

            // Assert
            mockLogger.Verify(x => x.LogError(It.Is<string>(s => s.Contains("AsyncFlushPageToDeviceCallback error")), 1), Times.Once);
        }
    }
}
