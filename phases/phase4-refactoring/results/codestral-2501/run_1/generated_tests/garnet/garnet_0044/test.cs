using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class ClusterUtilsTests
    {
        [Fact]
        public void IOCallback_LogsError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 1;
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "[ClusterUtils] OverlappedStream GetQueuedCompletionStatus error: {errorCode} msg: {errorMessage}",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void IOCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 10;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(loggerMock.Object, errorCode, numBytes, context);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public void ReadInto_CallsIOCallback()
        {
            // Arrange
            var deviceMock = new Mock<IDevice>();
            var poolMock = new Mock<SectorAlignedBufferPool>();
            var loggerMock = new Mock<ILogger>();
            ulong address = 0;
            int size = 10;
            var semaphore = new SemaphoreSlim(0);
            var pbuffer = new SectorAlignedBuffer(10, IntPtr.Zero);

            deviceMock.Setup(x => x.ReadAsync(It.IsAny<ulong>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<object>()))
                .Callback<ulong, IntPtr, uint, Action<uint, uint, object>, object>((addr, ptr, numBytes, callback, ctx) =>
                {
                    callback(0, numBytes, ctx);
                });

            poolMock.Setup(x => x.Get(It.IsAny<int>())).Returns(pbuffer);

            // Act
            ClusterUtils.ReadInto(deviceMock.Object, poolMock.Object, address, out byte[] buffer, size, loggerMock.Object);

            // Assert
            deviceMock.Verify(x => x.ReadAsync(It.IsAny<ulong>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void WriteInto_CallsIOCallback()
        {
            // Arrange
            var deviceMock = new Mock<IDevice>();
            var poolMock = new Mock<SectorAlignedBufferPool>();
            var loggerMock = new Mock<ILogger>();
            ulong address = 0;
            byte[] buffer = new byte[10];
            int size = 10;
            var semaphore = new SemaphoreSlim(0);
            var pbuffer = new SectorAlignedBuffer(10, IntPtr.Zero);

            deviceMock.Setup(x => x.WriteAsync(It.IsAny<IntPtr>(), It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<object>()))
                .Callback<IntPtr, ulong, uint, Action<uint, uint, object>, object>((ptr, addr, numBytes, callback, ctx) =>
                {
                    callback(0, numBytes, ctx);
                });

            poolMock.Setup(x => x.Get(It.IsAny<int>())).Returns(pbuffer);

            // Act
            ClusterUtils.WriteInto(deviceMock.Object, poolMock.Object, address, buffer, size, loggerMock.Object);

            // Assert
            deviceMock.Verify(x => x.WriteAsync(It.IsAny<IntPtr>(), It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<object>()), Times.Once);
        }
    }
}
