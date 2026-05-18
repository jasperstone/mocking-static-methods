using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsavorite.core
{
    public class RecoveryTests
    {
        [Fact]
        public async Task TestRecoveryLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new TsavoriteKV<int, int, MockStoreFunctions, MockAllocator>(loggerMock.Object);

            // Act
            await recovery.InternalRecoverAsync(new HybridLogCheckpointInfo(), new IndexCheckpointInfo(), 10, false, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once);
        }
    }

    public class MockStoreFunctions : IStoreFunctions<int, int>
    {
        public long GetSize(in int key)
        {
            return 0;
        }

        public long GetSize(in int key, in int value)
        {
            return 0;
        }

        public bool TryGet(in int key, out int value)
        {
            value = 0;
            return false;
        }

        public bool TryAdd(in int key, in int value)
        {
            return false;
        }

        public bool TryUpdate(in int key, in int value)
        {
            return false;
        }

        public bool TryRemove(in int key)
        {
            return false;
        }

        public long GetKeyHashCode64(ref int key)
        {
            return 0;
        }

        public bool KeysEqual(ref int key1, ref int key2)
        {
            return false;
        }

        public void BeginSerializeKey(Stream stream, in int key)
        {
        }

        public void BeginDeserializeKey(Stream stream, out int key)
        {
            key = 0;
        }

        public void BeginSerializeValue(Stream stream, in int value)
        {
        }

        public void BeginDeserializeValue(Stream stream, out int value)
        {
            value = 0;
        }

        public void DisposeRecord(ref int key, ref int value, DisposeReason reason, int version)
        {
        }
    }

    public class MockAllocator : IAllocator<int, int, MockStoreFunctions>
    {
        public int Allocate(in int key, in int value)
        {
            return 0;
        }

        public void Deallocate(in int key)
        {
        }

        public TAllocator GetBase<TAllocator>() where TAllocator : IAllocator<int, int, MockStoreFunctions>
        {
            return default;
        }

        public int GetAndInitializeValue(long address, long version)
        {
            return 0;
        }

        public int GetRMWCopyDestinationRecordSize<TInput, TVariableLengthInput>(ref int key, ref TInput input, ref int value, ref RecordInfo recordInfo, TVariableLengthInput variableLengthInput)
        {
            return 0;
        }

        public int GetRMWInitialRecordSize<TInput, TSessionFunctionsWrapper>(ref int key, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            return 0;
        }

        public int GetUpsertRecordSize<TInput, TSessionFunctionsWrapper>(ref int key, ref int value, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            return 0;
        }

        public int GetRecordSize(ref int key, ref int value)
        {
            return 0;
        }
    }
}
