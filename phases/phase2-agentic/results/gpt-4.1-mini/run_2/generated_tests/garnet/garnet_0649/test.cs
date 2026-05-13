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
            public long RecoverFromAddressOut { get; set; } = 10;
            public long TailAddressOut { get; set; } = 100;
            public long HeadAddressOut { get; set; } = 200;
            public long ScanFromAddressOut { get; set; } = 300;
            public long RecoverHybridLogReturnValue { get; set; } = 123;
            public long RecoverHybridLogFromSnapshotFileReturnValue { get; set; } = -1;
            public long RecoverHybridLogAsyncReturnValue { get; set; } = 123;
            public long RecoverHybridLogFromSnapshotFileAsyncReturnValue { get; set; } = -1;

            public TestTsavoriteKV()
            {
                // Setup logger to null by default
                Logger = null;
            }

            public override ILogger LoggerAccessor => Logger;

            public override long GetTailAddress() => TailAddressToReturn;

            public override long GetFirstValidLogicalAddress(int _) => FirstValidLogicalAddressToReturn;

            public override void Reset()
            {
                ResetCalled = true;
            }

            public override bool RecoverToInitialPage(IndexCheckpointInfo recoveredICInfo, HybridLogCheckpointInfo recoveredHLCInfo, out long recoverFromAddress)
            {
                recoverFromAddress = RecoverFromAddressOut;
                return RecoverToInitialPageResult;
            }

            public override Task RecoverFuzzyIndexAsync(IndexCheckpointInfo recoveredICInfo, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public override bool SetRecoveryPageRanges(HybridLogCheckpointInfo recoveredHLCInfo, int numPagesToPreload, long recoverFromAddress,
                out long tailAddress, out long headAddress, out long scanFromAddress)
            {
                tailAddress = TailAddressOut;
                headAddress = HeadAddressOut;
                scanFromAddress = ScanFromAddressOut;
                return SetRecoveryPageRangesResult;
            }

            public override Task<long> RecoverHybridLogAsync(long scanFromAddress, long recoverFromAddress, long finalLogicalAddress, long nextVersion,
                CheckpointType checkpointType, RecoveryOptions options, CancellationToken cancellationToken)
            {
                return Task.FromResult(RecoverHybridLogAsyncReturnValue);
            }

            public override Task<long> RecoverHybridLogFromSnapshotFileAsync(long flushedLogicalAddress, long recoverFromAddress, long finalLogicalAddress,
                long snapshotStartFlushedLogicalAddress, long snapshotFinalLogicalAddress, long nextVersion, Guid guid, RecoveryOptions options,
                object deltaLog, long recoverTo, CancellationToken cancellationToken)
            {
                return Task.FromResult(RecoverHybridLogFromSnapshotFileAsyncReturnValue);
            }

            public override long RecoverHybridLog(long scanFromAddress, long recoverFromAddress, long finalLogicalAddress, long nextVersion,
                CheckpointType checkpointType, RecoveryOptions options)
            {
                return RecoverHybridLogReturnValue;
            }

            public override long RecoverHybridLogFromSnapshotFile(long flushedLogicalAddress, long recoverFromAddress, long finalLogicalAddress,
                long snapshotStartFlushedLogicalAddress, long snapshotFinalLogicalAddress, long nextVersion, Guid guid, RecoveryOptions options,
                object deltaLog, long recoverTo)
            {
                return RecoverHybridLogFromSnapshotFileReturnValue;
            }

            public override void DoPostRecovery(IndexCheckpointInfo recoveredICInfo, HybridLogCheckpointInfo recoveredHLCInfo, long tailAddress,
                ref long headAddress, ref long readOnlyAddress, long lastFreedPage)
            {
                // no-op for test
            }
        }

        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenTailAddressGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteKV
            {
                Logger = loggerMock.Object,
                TailAddressToReturn = 100,
                FirstValidLogicalAddressToReturn = 50,
                RecoverToInitialPageResult = true,
                SetRecoveryPageRangesResult = true,
                RecoverHybridLogAsyncReturnValue = 42,
                RecoverHybridLogFromSnapshotFileAsyncReturnValue = -1
            };

            var recoveredICInfo = default(IndexCheckpointInfo);
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var cancellationToken = CancellationToken.None;

            // Act
            var version = await tsavorite.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log - resetting to empty state first")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(tsavorite.ResetCalled);
        }

        [Fact]
        public async Task InternalRecoverAsync_DoesNotLog_WhenTailAddressNotGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteKV
            {
                Logger = loggerMock.Object,
                TailAddressToReturn = 40,
                FirstValidLogicalAddressToReturn = 50,
                RecoverToInitialPageResult = true,
                SetRecoveryPageRangesResult = true,
                RecoverHybridLogAsyncReturnValue = 42,
                RecoverHybridLogFromSnapshotFileAsyncReturnValue = -1
            };

            var recoveredICInfo = default(IndexCheckpointInfo);
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var cancellationToken = CancellationToken.None;

            // Act
            var version = await tsavorite.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            Assert.False(tsavorite.ResetCalled);
        }
    }
}
