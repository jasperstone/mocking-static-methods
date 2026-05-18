using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.core
{
    public class RecoveryLoggingTests
    {
        // Minimal implementation of IStoreFunctions<int,int>
        private class DummyStoreFunctions : IStoreFunctions<int, int>
        {
            public bool KeysEqual(ref int k1, ref int k2) => k1 == k2;
            public long GetKeyHashCode64(ref int key) => key.GetHashCode();
            public void BeginSerializeKey(Stream stream) { }
            public void EndSerializeKey(Stream stream) { }
            public void BeginDeserializeKey(Stream stream) { }
            public void EndDeserializeKey(Stream stream) { }
            public void BeginSerializeValue(Stream stream) { }
            public void EndSerializeValue(Stream stream) { }
            public void BeginDeserializeValue(Stream stream) { }
            public void EndDeserializeValue(Stream stream) { }
        }

        // Minimal implementation of IAllocator<int,int,DummyStoreFunctions>
        private class DummyAllocator : IAllocator<int, int, DummyStoreFunctions>
        {
            public DummyStoreFunctions StoreFunctions { get; } = new DummyStoreFunctions();
            public void Dispose() { }
            public void Initialize() { }
            public void Allocate() { }
            public void Free() { }
        }

        private class TestTsavoriteKV : TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>
        {
            public TestTsavoriteKV(ILogger logger)
            {
                this.logger = logger;
            }

            public bool ResetCalled { get; private set; }

            protected override long GetTailAddress() => 100; // Non-empty log triggers logging

            protected override long GetFirstValidLogicalAddress(int _) => 0;

            protected override void Reset()
            {
                ResetCalled = true;
            }

            protected override bool RecoverToInitialPage(object recoveredICInfo, object recoveredHLCInfo, out long recoverFromAddress)
            {
                recoverFromAddress = 0;
                return true;
            }

            protected override Task RecoverFuzzyIndexAsync(object recoveredICInfo, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            protected override bool SetRecoveryPageRanges(object recoveredHLCInfo, int numPagesToPreload, long recoverFromAddress, out long tailAddress, out long headAddress, out long scanFromAddress)
            {
                tailAddress = 100;
                headAddress = 0;
                scanFromAddress = 0;
                return true;
            }

            protected override Task<long> RecoverHybridLogAsync(long scanFromAddress, long recoverFromAddress, long finalLogicalAddress, long nextVersion, CheckpointType checkpointType, object options, CancellationToken cancellationToken)
            {
                return Task.FromResult(0L);
            }

            protected override Task<long> RecoverHybridLogFromSnapshotFileAsync(long flushedLogicalAddress, long recoverFromAddress, long finalLogicalAddress, long snapshotStartFlushedLogicalAddress, long snapshotFinalLogicalAddress, long nextVersion, Guid guid, object options, object deltaLog, long recoverTo, CancellationToken cancellationToken)
            {
                return Task.FromResult(-1L);
            }

            protected override void DoPostRecovery(object recoveredICInfo, object recoveredHLCInfo, long tailAddress, ref long headAddress, ref long readOnlyAddress, long lastFreedPage)
            {
                // no-op
            }
        }

        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNonEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteKV(loggerMock.Object);

            // Use null for internal types to avoid accessibility issues
            object recoveredICInfo = null;
            object recoveredHLCInfo = null;
            int numPagesToPreload = 0;
            bool undoNextVersion = false;
            long recoverTo = 0;
            var cancellationToken = CancellationToken.None;

            // Use reflection to invoke private InternalRecoverAsync
            var method = typeof(TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>)
                .GetMethod("InternalRecoverAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(method);

            var task = (ValueTask<long>)method.Invoke(tsavorite, new object[] { recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken });
            await task.AsTask();

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
        }
    }
}
