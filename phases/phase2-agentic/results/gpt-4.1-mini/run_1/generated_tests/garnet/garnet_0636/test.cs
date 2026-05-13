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
            public long GetFileSize(int index) => 4096;
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
            var loggerMock = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 9, 0, loggerMock.Object);

            // Use reflection to get private method AsyncFlushPageToDeviceCallback
            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Create a dummy context with count = 1
            var flushResult = new DummyPageAsyncFlushResult();

            // Act
            // Call with errorCode != 0 to trigger LogError call
            method.Invoke(deltaLog, new object[] { (uint)123, (uint)0, flushResult });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error:")),
                    123,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotThrow_WhenDisposedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 9, 0, loggerMock.Object);

            // Set disposed to true via reflection
            var disposedField = typeof(DeltaLog).GetField("disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            disposedField.SetValue(deltaLog, true);

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var flushResult = new DummyPageAsyncFlushResult();

            // Act & Assert: Should not throw even if disposed is true
            var ex = Record.Exception(() => method.Invoke(deltaLog, new object[] { (uint)1, (uint)0, flushResult }));
            Assert.Null(ex);
        }
    }
}
