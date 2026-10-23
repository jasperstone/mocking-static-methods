using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Tsavorite.core
{
    public class RecoveryTests
    {
        [Fact]
        public void LogInformation_Called_When_RecoveryCalledOnNonEmptyLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new TsavoriteKV<int, int, StoreFunctions, Allocator>(loggerMock.Object);

            // Act
            recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, default);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once);
        }
    }

    public class StoreFunctions : IStoreFunctions<int, int>
    {
        public long GetKeyHashCode64(ref int key)
        {
            throw new NotImplementedException();
        }

        public bool KeysEqual(ref int key1, ref int key2)
        {
            throw new NotImplementedException();
        }

        public IObjectSerializer<int> BeginSerializeKey(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public IObjectSerializer<int> BeginDeserializeKey(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public IObjectSerializer<int> BeginSerializeValue(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public IObjectSerializer<int> BeginDeserializeValue(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public int Get(int key)
        {
            throw new NotImplementedException();
        }

        public void Put(int key, int value)
        {
            throw new NotImplementedException();
        }

        public void Delete(int key)
        {
            throw new NotImplementedException();
        }

        public void DisposeRecord(ref int key, ref int value, DisposeReason reason, int version)
        {
            throw new NotImplementedException();
        }

        public void SetCheckpointCompletedCallback(Action callback)
        {
            throw new NotImplementedException();
        }

        public void OnCheckpointCompleted()
        {
            throw new NotImplementedException();
        }

        public bool HasKeySerializer => throw new NotImplementedException();

        public bool HasValueSerializer => throw new NotImplementedException();

        public bool DisposeOnPageEviction => throw new NotImplementedException();
    }

    public class Allocator : IAllocator<int, int, StoreFunctions>
    {
        public int Allocate()
        {
            throw new NotImplementedException();
        }

        public void Deallocate(int handle)
        {
            throw new NotImplementedException();
        }

        public Allocator GetBase<Allocator>()
        {
            throw new NotImplementedException();
        }
    }
}
