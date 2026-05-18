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
        // Dummy implementations for generic constraints
        private class DummyStoreFunctions : IStoreFunctions<int, int>
        {
            public long GetKeyHashCode64(ref int key) => key.GetHashCode();
            public bool KeysEqual(ref int k1, ref int k2) => k1 == k2;
            public void BeginSerializeKey(System.IO.Stream stream) { }
            public void BeginDeserializeKey(System.IO.Stream stream) { }
            public void BeginSerializeValue(System.IO.Stream stream) { }
            public void BeginDeserializeValue(System.IO.Stream stream) { }
            public void SerializeKey(ref int key, long physicalAddress) { }
            public void SerializeValue(ref int value, long physicalAddress) { }
            public void DeserializeKey(out int key, long physicalAddress) => key = 0;
            public void DeserializeValue(out int value, long physicalAddress) => value = 0;
        }

        private class DummyAllocator : IAllocator<int, int, DummyStoreFunctions>
        {
            public AllocatorBase<int, int, DummyStoreFunctions, TAllocator> GetBase<TAllocator>() where TAllocator : IAllocator<int, int, DummyStoreFunctions> => null!;
            public bool IsFixedLength => true;
            public bool HasObjectLog => false;
            public ref int GetAndInitializeValue(long physicalAddress, long endPhysicalAddress) => throw new NotImplementedException();
            public (int actualSize, int allocatedSize, int keySize) GetRMWCopyDestinationRecordSize<TInput, TVariableLengthInput>(ref int key, ref TInput input, ref int value, ref RecordInfo recordInfo, TVariableLengthInput varlenInput) where TVariableLengthInput : IVariableLengthInput<int, TInput> => (0, 0, 0);
            public (int actualSize, int allocatedSize, int keySize) GetRMWInitialRecordSize<TInput, TSessionFunctionsWrapper>(ref int key, ref TInput input, TSessionFunctionsWrapper sessionFunctions) where TSessionFunctionsWrapper : IVariableLengthInput<int, TInput> => (0, 0, 0);
            public (int actualSize, int allocatedSize, int keySize) GetUpsertRecordSize<TInput, TSessionFunctionsWrapper>(ref int key, ref int value, ref TInput input, TSessionFunctionsWrapper sessionFunctions) where TSessionFunctionsWrapper : IVariableLengthInput<int, TInput> => (0, 0, 0);
            public (int actualSize, int allocatedSize, int keySize) GetRecordSize(ref int key, ref int value) => (0, 0, 0);
            public (int actualSize, int allocatedSize, int keySize) GetTombstoneRecordSize(ref int key) => (0, 0, 0);
            public int GetValueLength(ref int value) => 0;
            public void MarkPage(long logicalAddress, long version) { }
            public void MarkPageAtomic(long logicalAddress, long version) { }
            public long[] GetSegmentOffsets() => Array.Empty<long>();
            public void SerializeKey(ref int key, long physicalAddress) { }
        }

        private class TestTsavoriteKV : TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>
        {
            public TestTsavoriteKV(ILogger logger) : base()
            {
                this.logger = logger;
            }

            public async Task<long> CallInternalRecoverAsync(
                IndexCheckpointInfo recoveredICInfo,
                HybridLogCheckpointInfo recoveredHLCInfo,
                int numPagesToPreload,
                bool undoNextVersion,
                long recoverTo,
                CancellationToken cancellationToken)
            {
                var method = typeof(TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>).GetMethod("InternalRecoverAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var task = (ValueTask<long>)method.Invoke(this, new object[] { recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken });
                return await task;
            }
        }

        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenTailAddressGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var tsavorite = new TestTsavoriteKV(loggerMock.Object);

            var hlogBaseField = typeof(TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>).GetField("hlogBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hlogField = typeof(TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>).GetField("hlog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var hlogBaseMock = new Mock<IHLogBase>();
            var hlogMock = new Mock<IHLog>();

            hlogBaseMock.Setup(h => h.VerifyRecoveryInfo(It.IsAny<HybridLogCheckpointInfo>(), false));
            hlogBaseMock.Setup(h => h.GetTailAddress()).Returns(1000L);
            hlogMock.Setup(h => h.GetFirstValidLogicalAddress(0)).Returns(500L);

            hlogBaseField.SetValue(tsavorite, hlogBaseMock.Object);
            hlogField.SetValue(tsavorite, hlogMock.Object);

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var cancellationToken = CancellationToken.None;

            // Act
            try
            {
                await tsavorite.CallInternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0L, cancellationToken);
            }
            catch
            {
                // Ignore exceptions, focus on logging verification
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Dummy interfaces to satisfy references
    internal interface IHLogBase
    {
        void VerifyRecoveryInfo(HybridLogCheckpointInfo info, bool flag);
        long GetTailAddress();
        int LogPageSizeBits { get; }
    }

    internal interface IHLog
    {
        long GetFirstValidLogicalAddress(int param);
    }

    internal struct IndexCheckpointInfo { }

    internal struct HybridLogCheckpointInfo : IDisposable
    {
        public HybridLogCheckpointInfoInfo info;

        public void Dispose() { }

        public void Recover(Guid token, object checkpointManager, int logPageSizeBits, out object something, bool flag)
        {
            something = null;
        }
    }

    internal struct HybridLogCheckpointInfoInfo
    {
        public long finalLogicalAddress;
        public long nextVersion;
        public int useSnapshotFile;
        public long flushedLogicalAddress;
        public long startLogicalAddress;
        public long snapshotStartFlushedLogicalAddress;
        public long snapshotFinalLogicalAddress;
        public Guid guid;
    }
}
