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
        private class TestTsavoriteKV : TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>
        {
            public Mock<ILogger> LoggerMock { get; } = new Mock<ILogger>();

            public TestTsavoriteKV()
            {
                this.logger = LoggerMock.Object;
            }

            // Expose InternalRecoverAsync for testing
            public async Task<long> CallInternalRecoverAsync(
                IndexCheckpointInfo recoveredICInfo,
                HybridLogCheckpointInfo recoveredHLCInfo,
                int numPagesToPreload,
                bool undoNextVersion,
                long recoverTo,
                CancellationToken cancellationToken)
            {
                return await InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken);
            }

            // Override or mock dependencies as needed
            protected override long GetTailAddress() => 1000; // non-empty log
            protected override long GetFirstValidLogicalAddress(int _) => 0;

            protected override void Reset()
            {
                ResetCalled = true;
            }

            public bool ResetCalled { get; private set; } = false;

            protected override bool RecoverToInitialPage(IndexCheckpointInfo recoveredICInfo, HybridLogCheckpointInfo recoveredHLCInfo, out long recoverFromAddress)
            {
                recoverFromAddress = 0;
                return true;
            }

            protected override bool SetRecoveryPageRanges(HybridLogCheckpointInfo recoveredHLCInfo, int numPagesToPreload, long recoverFromAddress,
                out long tailAddress, out long headAddress, out long scanFromAddress)
            {
                tailAddress = 2000;
                headAddress = 100;
                scanFromAddress = 50;
                return true;
            }

            protected override Task<long> RecoverHybridLogAsync(long scanFromAddress, long recoverFromAddress, long finalLogicalAddress, long nextVersion,
                CheckpointType checkpointType, RecoveryOptions options, CancellationToken cancellationToken)
            {
                return Task.FromResult(123L);
            }

            protected override Task<long> RecoverHybridLogFromSnapshotFileAsync(long flushedLogicalAddress, long recoverFromAddress, long finalLogicalAddress,
                long snapshotStartFlushedLogicalAddress, long snapshotFinalLogicalAddress, long nextVersion, Guid guid, RecoveryOptions options,
                object deltaLog, long recoverTo, CancellationToken cancellationToken)
            {
                return Task.FromResult(-1L);
            }
        }

        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNonEmpty()
        {
            // Arrange
            var kv = new TestTsavoriteKV();

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            // Setup recoveredHLCInfo.info.useSnapshotFile = 0 to take the first branch
            recoveredHLCInfo.info = new HybridLogCheckpointInfo.InfoStruct
            {
                useSnapshotFile = 0,
                finalLogicalAddress = 5000,
                nextVersion = 1,
                flushedLogicalAddress = 0,
                startLogicalAddress = 0
            };

            var cancellationToken = CancellationToken.None;

            // Act
            var version = await kv.CallInternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken);

            // Assert
            kv.LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(kv.ResetCalled);
            Assert.Equal(1, version);
        }
    }
}
