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
            var context = new object();
            var result = new PageAsyncFlushResult<Empty> { count = 1, Free = () => { } };
            deltaLog.completedSemaphore = new SemaphoreSlim(0);
            deltaLog.issuedFlush = 1;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(1, 0, context);

            // Assert
            mockLogger.Verify(x => x.LogError(It.Is<string>(s => s.Contains("AsyncFlushPageToDeviceCallback error")), 1), Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DecrementsCountAndReleasesSemaphore_WhenNoError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);
            var context = new object();
            var result = new PageAsyncFlushResult<Empty> { count = 2, Free = () => { } };
            deltaLog.completedSemaphore = new SemaphoreSlim(0);
            deltaLog.issuedFlush = 1;

            // Act
            deltaLog.AsyncFlushPageToDeviceCallback(0, 0, result);

            // Assert
            Assert.Equal(1, result.count);
            Assert.Equal(1, deltaLog.issuedFlush);
            Assert.Equal(1, deltaLog.completedSemaphore.CurrentCount);
        }
    }
}
