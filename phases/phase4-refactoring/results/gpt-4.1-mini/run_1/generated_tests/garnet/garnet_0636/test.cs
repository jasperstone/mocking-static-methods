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
            public uint SectorSize => 512;
            public string FileName => "dummy";
            public long Capacity => 4096;
            public long SegmentSize => 4096;
            public int StartSegment => 0;
            public int EndSegment => 0;
            public int ThrottleLimit { get; set; }

            public void Dispose() { }
            public void Initialize(long segmentSize, LightEpoch epoch = null, bool omitSegmentIdFromFilename = false) { }
            public bool TryComplete() => true;
            public bool Throttle() => false;

            public void WriteAsync(IntPtr sourceAddress, int segmentId, ulong destinationAddress, uint numBytesToWrite, DeviceIOCompletionCallback callback, object context) { }
            public void ReadAsync(int segmentId, ulong sourceAddress, IntPtr destinationAddress, uint readLength, DeviceIOCompletionCallback callback, object context) { }
            public void WriteAsync(IntPtr alignedSourceAddress, ulong alignedDestinationAddress, uint numBytesToWrite, DeviceIOCompletionCallback callback, object context) { }
            public void ReadAsync(ulong alignedSourceAddress, IntPtr alignedDestinationAddress, uint aligned_read_length, DeviceIOCompletionCallback callback, object context) { }
            public void TruncateUntilAddressAsync(long toAddress, AsyncCallback callback, IAsyncResult result) { }
            public void TruncateUntilAddress(long toAddress) { }
            public void TruncateUntilSegmentAsync(int toSegment, AsyncCallback callback, IAsyncResult result) { }
            public void TruncateUntilSegment(int toSegment) { }
            public void RemoveSegmentAsync(int segment, AsyncCallback callback, IAsyncResult result) { }
            public void RemoveSegment(int segment) { }
            public long GetFileSize(int index) => 4096;
            public void Reset() { }
        }

        private class DummyPageAsyncFlushResult
        {
            public int count = 1;
            public void Free() { }
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeNonZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 9, 0, mockLogger.Object);

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            uint errorCode = 123; // non-zero error code to trigger LogError
            uint numBytes = 0;
            var context = new DummyPageAsyncFlushResult();

            // Act
            method.Invoke(deltaLog, new object[] { errorCode, numBytes, context });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AsyncFlushPageToDeviceCallback error")),
                    It.IsAny<object>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var device = new DummyDevice();
            var deltaLog = new DeltaLog(device, 9, 0, mockLogger.Object);

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            uint errorCode = 0; // zero error code means no error
            uint numBytes = 0;
            var context = new DummyPageAsyncFlushResult();

            // Act
            method.Invoke(deltaLog, new object[] { errorCode, numBytes, context });

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<object>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
        }
    }
}
