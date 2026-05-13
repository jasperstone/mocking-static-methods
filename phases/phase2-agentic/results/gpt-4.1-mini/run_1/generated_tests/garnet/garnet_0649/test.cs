using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        private class TestTsavoriteKV : TsavoriteKV<int, int, object, object>
        {
            public ILogger Logger { get; set; }
            public bool ResetCalled { get; private set; }
            public long TailAddressToReturn { get; set; } = 100;
            public long FirstValidLogicalAddressToReturn { get; set; } = 50;
            public bool RecoverToInitialPageResult { get; set; } = true;
            public bool SetRecoveryPageRangesResult { get; set; } = true;
            public long RecoverFromAddress { get; set; } = 0;
            public long TailAddressOut { get; set; }
            public long HeadAddressOut { get; set; }
            public long ScanFromAddressOut { get; set; }
            public long RecoverHybridLogAsyncCalledCount { get; private set; }
            public long RecoverHybridLogFromSnapshotFileAsyncCalledCount { get; private set; }

            public TestTsavoriteKV()
            {
                // Setup logger property in base class
                this.logger = Logger;
            }

            public void SetLogger(ILogger logger)
            {
                this.logger = logger;
            }

            public void SetHlogBaseTailAddress(long tailAddress)
            {
                hlogBaseTailAddress = tailAddress;
            }

            public void SetHlogFirstValidLogicalAddress(long firstValidLogicalAddress)
            {
                hlogFirstValidLogicalAddress = firstValidLogicalAddress;
            }

            public void SetRecoverToInitialPageResult(bool result)
            {
                recoverToInitialPageResult = result;
            }

            public void SetSetRecoveryPageRangesResult(bool result)
            {
                setRecoveryPageRangesResult = result;
            }

            public void SetRecoveryPageRangesOut(long tail, long head, long scan)
            {
                tailAddressOut = tail;
                headAddressOut = head;
                scanFromAddressOut = scan;
            }

            public void Reset()
            {
                ResetCalled = true;
            }

            public long GetTailAddress()
            {
                return hlogBaseTailAddress;
            }

            public long GetFirstValidLogicalAddress(int param)
            {
                return hlogFirstValidLogicalAddress;
            }

            public bool RecoverToInitialPage(IndexCheckpointInfo recoveredICInfo, HybridLogCheckpointInfo recoveredHLCInfo, out long recoverFromAddress)
            {
                recoverFromAddress = RecoverFromAddress;
                return recoverToInitialPageResult;
            }

            public Task<long> RecoverHybridLogAsync(long scanFromAddress, long recoverFromAddress, long finalLogicalAddress, long nextVersion, CheckpointType checkpointType, RecoveryOptions options, CancellationToken cancellationToken)
            {
                RecoverHybridLogAsyncCalledCount++;
                return Task.FromResult(42L);
            }

            public Task<long> RecoverHybridLogFromSnapshotFileAsync(long flushedLogicalAddress, long recoverFromAddress, long finalLogicalAddress, long snapshotStartFlushedLogicalAddress, long snapshotFinalLogicalAddress, long nextVersion, Guid guid, RecoveryOptions options, object deltaLog, long recoverTo, CancellationToken cancellationToken)
            {
                RecoverHybridLogFromSnapshotFileAsyncCalledCount++;
                return Task.FromResult(43L);
            }

            public bool SetRecoveryPageRanges(HybridLogCheckpointInfo recoveredHLCInfo, int numPagesToPreload, long recoverFromAddress, out long tailAddress, out long headAddress, out long scanFromAddress)
            {
                tailAddress = tailAddressOut;
                headAddress = headAddressOut;
                scanFromAddress = scanFromAddressOut;
                return setRecoveryPageRangesResult;
            }

            public Task RecoverFuzzyIndexAsync(IndexCheckpointInfo recoveredICInfo, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            // Fields to simulate internal state
            private ILogger logger;
            private long hlogBaseTailAddress = 0;
            private long hlogFirstValidLogicalAddress = 0;
            private bool recoverToInitialPageResult = true;
            private bool setRecoveryPageRangesResult = true;
            private long tailAddressOut = 0;
            private long headAddressOut = 0;
            private long scanFromAddressOut = 0;
        }

        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenTailAddressGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteKV();
            tsavorite.SetLogger(loggerMock.Object);
            tsavorite.SetHlogBaseTailAddress(100);
            tsavorite.SetHlogFirstValidLogicalAddress(50);
            tsavorite.SetRecoverToInitialPageResult(true);
            tsavorite.SetSetRecoveryPageRangesResult(true);
            tsavorite.SetRecoveryPageRangesOut(200, 300, 400);

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            recoveredHLCInfo.info = new HybridLogCheckpointInfo.Info
            {
                useSnapshotFile = 0,
                finalLogicalAddress = 500,
                nextVersion = 1,
                flushedLogicalAddress = 0,
                startLogicalAddress = 0
            };

            var cancellationToken = CancellationToken.None;

            // Act
            var version = await tsavorite.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(tsavorite.ResetCalled);
            Assert.Equal(1, version);
        }

        [Fact]
        public async Task InternalRecoverAsync_DoesNotLog_WhenTailAddressNotGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteKV();
            tsavorite.SetLogger(loggerMock.Object);
            tsavorite.SetHlogBaseTailAddress(40);
            tsavorite.SetHlogFirstValidLogicalAddress(50);
            tsavorite.SetRecoverToInitialPageResult(true);
            tsavorite.SetSetRecoveryPageRangesResult(true);
            tsavorite.SetRecoveryPageRangesOut(200, 300, 400);

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            recoveredHLCInfo.info = new HybridLogCheckpointInfo.Info
            {
                useSnapshotFile = 0,
                finalLogicalAddress = 500,
                nextVersion = 1,
                flushedLogicalAddress = 0,
                startLogicalAddress = 0
            };

            var cancellationToken = CancellationToken.None;

            // Act
            var version = await tsavorite.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            Assert.False(tsavorite.ResetCalled);
            Assert.Equal(1, version);
        }
    }
}
