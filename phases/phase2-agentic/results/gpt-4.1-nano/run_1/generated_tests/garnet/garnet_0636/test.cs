using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
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
            public void Dispose() { }
        }

        [Fact]
        public async Task AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1, Free = () => { } };
            deltaLog.completedSemaphore = new SemaphoreSlim(0);
            deltaLog.issuedFlush = 1;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(1, 0, context);

            // Assert
            mockLogger.Verify(x => x.LogError(It.Is<string>(s => s.Contains("AsyncFlushPageToDeviceCallback error")), 1), Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DecrementsCountAndReleasesSemaphore_WhenNoMorePages()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1, Free = () => { } };
            deltaLog.completedSemaphore = new SemaphoreSlim(0);
            deltaLog.issuedFlush = 1;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);

            // Assert
            Assert.Equal(0, context.count);
            Assert.Equal(1, deltaLog.issuedFlush);
            Assert.Equal(1, deltaLog.completedSemaphore.CurrentCount);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_CatchesDisposedException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);
            var context = new PageAsyncFlushResult<Empty> { count = 1, Free = () => { } };
            deltaLog.completedSemaphore = new SemaphoreSlim(0);
            deltaLog.issuedFlush = 1;
            deltaLog.disposed = true;

            // Act & Assert: should not throw
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, context);
        }
    }

    // Helper class to simulate page flush result
    public class PageAsyncFlushResult<T>
    {
        public int count;
        public Action Free;
    }
}
