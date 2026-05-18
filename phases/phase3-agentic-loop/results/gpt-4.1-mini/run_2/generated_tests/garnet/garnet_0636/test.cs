using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class DeltaLogTests
    {
        private class DummyDevice : IDevice
        {
            public ulong SectorSize => 512;
            public long GetFileSize(int fileId) => 4096;
            public void ReadAsync(ulong offset, IntPtr buffer, uint length, Action<uint, uint, object> callback, object context)
            {
                // Simulate async read callback with no error
                callback(0, length, context);
            }
        }

        private class DummyPageAsyncFlushResult : PageAsyncFlushResult<Empty>
        {
            public DummyPageAsyncFlushResult()
            {
                count = 1;
            }
            public override void Free()
            {
                // no-op
            }
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);

            // Use reflection to get private method AsyncFlushPageToDeviceCallback
            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Create a dummy context with count = 1
            var context = new DummyPageAsyncFlushResult();

            // Act
            // Call with errorCode != 0 to trigger LogError
            method.Invoke(deltaLog, new object[] { (uint)123, (uint)0, context });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error:")),
                    123,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 12, 0, mockLogger.Object);

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var context = new DummyPageAsyncFlushResult();

            // Act
            method.Invoke(deltaLog, new object[] { (uint)0, (uint)0, context });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
