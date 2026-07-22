using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Tsavorite.core;
using Xunit;

public class DeltaLogTests
{
    private class MockDevice : IDevice
    {
        public uint SectorSize => 512;
        public string FileName => "test";
        public long Capacity => long.MaxValue;
        public long SegmentSize => 1L << 30;
        public int StartSegment => 0;
        public int EndSegment => int.MaxValue;
        public int ThrottleLimit { get; set; } = 1024;

        public void Initialize(long segmentSize, LightEpoch epoch = null, bool omitSegmentIdFromFilename = false) { }
        public bool TryComplete() => true;
        public bool Throttle() => false;
        public long GetFileSize(int threadId) => 0;
        public void Reset() { }
        public void RemoveSegment(int segment) { }

        public void WriteAsync(IntPtr sourceAddress, int segmentId, ulong destinationAddress, uint numBytesToWrite, DeviceIOCompletionCallback callback, object context) { }
        public void ReadAsync(int segmentId, ulong sourceAddress, IntPtr destinationAddress, uint readLength, DeviceIOCompletionCallback callback, object context) { }
        public void WriteAsync(IntPtr alignedSourceAddress, ulong alignedDestinationAddress, uint numBytesToWrite, DeviceIOCompletionCallback callback, object context) { }
        public void ReadAsync(ulong alignedSourceAddress, IntPtr alignedDestinationAddress, uint aligned_read_length, DeviceIOCompletionCallback callback, object context) { }

        public void TruncateUntilAddressAsync(long toAddress, AsyncCallback callback, IAsyncResult result) { }
        public void TruncateUntilAddress(long toAddress) { }
        public void TruncateUntilSegmentAsync(int toSegment, AsyncCallback callback, IAsyncResult result) { }
        public void TruncateUntilSegment(int toSegment) { }
        public void RemoveSegmentAsync(int segment, AsyncCallback callback, IAsyncResult result) { }

        public void Dispose() { }
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_LogsError_WhenErrorCodeIsNonZero()
    {
        // Arrange - Use reflection to access private method
        var loggerMock = new Mock<ILogger<DeltaLog>>();
        loggerMock.Setup(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Error), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

        var device = new MockDevice();
        var deltaLog = new DeltaLog(device, 12, 0, loggerMock.Object);

        var result = new PageAsyncFlushResult<Empty>();
        uint errorCode = 1001;
        uint numBytes = 4096;

        var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(deltaLog, new object[] { errorCode, numBytes, result });

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AsyncFlushPageToDeviceCallback error: 1001")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void AsyncFlushPageToDeviceCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DeltaLog>>();

        var device = new MockDevice();
        var deltaLog = new DeltaLog(device, 12, 0, loggerMock.Object);

        var result = new PageAsyncFlushResult<Empty>();
        uint errorCode = 0;
        uint numBytes = 4096;

        var method = typeof(DeltaLog).GetMethod("AsyncFlushPageToDeviceCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(deltaLog, new object[] { errorCode, numBytes, result });

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
}
