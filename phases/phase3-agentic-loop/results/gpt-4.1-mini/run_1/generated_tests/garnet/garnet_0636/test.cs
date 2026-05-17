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

            // Added missing members
            public long GetFileSize(int fileId) => 4096;
            public void Reset() { }
        }

        private class DummyPageAsyncFlushResult
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

            var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            var dummyContext = new DummyPageAsyncFlushResult { count = 1 };

            // Act
            method.Invoke(deltaLog, new object[] { (uint)123, (uint)0, dummyContext });

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

            var dummyContext = new DummyPageAsyncFlushResult { count = 1 };

            // Act
            method.Invoke(deltaLog, new object[] { (uint)0, (uint)0, dummyContext });

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
