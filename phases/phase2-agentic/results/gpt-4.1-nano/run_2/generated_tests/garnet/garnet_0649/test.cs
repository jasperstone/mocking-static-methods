using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryLoggingTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNotEmpty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>>>();
            var mockHlogBase = new Mock<IHlogBase>();
            var mockCheckpointManager = new Mock<ICheckpointManager>();
            var mockHlog = new Mock<IHybridLog>();
            var mockRecoveryDevice = new Mock<IDevice>();
            var mockObjectLogRecoveryDevice = new Mock<IDevice>();

            var kv = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>>();
            // Use reflection or other means to set private fields if necessary
            // For simplicity, assume we can set the logger directly
            kv.GetType().GetProperty("logger").SetValue(kv, mockLogger.Object);

            var recoveryInfo = new HybridLogCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();

            // Setup the mock to simulate non-empty log
            mockHlogBase.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(100);
            mockHlogBase.Setup(h => h.LogPageSizeBits).Returns(12);
            mockHlog.Setup(h => h.GetTailAddress()).Returns(200);
            mockHlog.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(100);
            mockHlog.Setup(h => h.VerifyRecoveryInfo(It.IsAny<HybridLogCheckpointInfo>(), false));
            mockHlog.Setup(h => h.Recover(It.IsAny<Guid>(), It.IsAny<ICheckpointManager>(), It.IsAny<int>(), out _, true))
                .Returns(true);

            // Act
            // Call InternalRecoverAsync with a cancellation token
            var recoveryMethod = typeof(TsavoriteKV<int, int, IStoreFunctions<int, int>>, IAllocator<int, int, IStoreFunctions<int, int>>>)
                .GetMethod("InternalRecoverAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)recoveryMethod.Invoke(kv, new object[]
            {
                null, // IndexCheckpointInfo
                null, // HybridLogCheckpointInfo
                10, // numPagesToPreload
                false, // undoNextVersion
                0L, // recoverTo
                CancellationToken.None
            });
            await task;

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
