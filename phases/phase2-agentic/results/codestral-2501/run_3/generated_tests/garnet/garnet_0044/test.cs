using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Garnet.cluster.Tests
{
    public class ClusterUtilsTests
    {
        [Fact]
        public void ReadDevice_ShouldReturnExpectedData()
        {
            // Arrange
            var mockDevice = new Mock<IDevice>();
            var mockPool = new Mock<SectorAlignedBufferPool>();
            var mockLogger = new Mock<ILogger>();

            var expectedData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var writePad = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 };

            mockDevice.Setup(d => d.ReadAsync(It.IsAny<ulong>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<SemaphoreSlim>()))
                .Callback<ulong, IntPtr, uint, Action<uint, uint, object>, SemaphoreSlim>((address, buffer, numBytes, callback, semaphore) =>
                {
                    // Simulate successful read
                    callback(0, (uint)writePad.Length, semaphore);
                });

            mockPool.Setup(p => p.Get(It.IsAny<int>())).Returns(new SectorAlignedBuffer { aligned_pointer = IntPtr.Zero });

            // Act
            var result = ClusterUtils.ReadDevice(mockDevice.Object, mockPool.Object, mockLogger.Object);

            // Assert
            Assert.Equal(expectedData, result);
        }

        [Fact]
        public void WriteInto_ShouldWriteExpectedData()
        {
            // Arrange
            var mockDevice = new Mock<IDevice>();
            var mockPool = new Mock<SectorAlignedBufferPool>();
            var mockLogger = new Mock<ILogger>();

            var dataToWrite = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var expectedData = new byte[] { 0x04, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 };

            mockDevice.Setup(d => d.WriteAsync(It.IsAny<IntPtr>(), It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<SemaphoreSlim>()))
                .Callback<IntPtr, ulong, uint, Action<uint, uint, object>, SemaphoreSlim>((buffer, address, numBytes, callback, semaphore) =>
                {
                    // Simulate successful write
                    callback(0, (uint)expectedData.Length, semaphore);
                });

            mockPool.Setup(p => p.Get(It.IsAny<int>())).Returns(new SectorAlignedBuffer { aligned_pointer = IntPtr.Zero });

            // Act
            ClusterUtils.WriteInto(mockDevice.Object, mockPool.Object, 0, dataToWrite, 0, mockLogger.Object);

            // Assert
            mockDevice.Verify(d => d.WriteAsync(It.IsAny<IntPtr>(), It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<Action<uint, uint, object>>(), It.IsAny<SemaphoreSlim>()), Times.Once);
        }

        [Fact]
        public void IOCallback_ShouldLogError_WhenErrorCodeIsNotZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            uint errorCode = 1;
            uint numBytes = 0;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public void IOCallback_ShouldNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            uint errorCode = 0;
            uint numBytes = 0;
            var context = new SemaphoreSlim(0);

            // Act
            LoggerExtensions.IOCallback(mockLogger.Object, errorCode, numBytes, context);

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Never);
        }
    }
}
