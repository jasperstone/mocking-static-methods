using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void LogInformation_Called_When_CheckpointManager_And_CheckpointDir_Specified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kvSettings = new KVSettings<string, string>
            {
                CheckpointSettings = new CheckpointSettings
                {
                    CheckpointDir = "/path/to/checkpoint/dir",
                    CheckpointManager = new DeviceLogCommitCheckpointManager(
                        new LocalStorageNamedDeviceFactoryCreator(),
                        new DefaultCheckpointNamingScheme(
                            new DirectoryInfo("/path/to/checkpoint/dir").FullName),
                        removeOutdated: true)
                }
            };

            var tsavoriteKV = new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
                kvSettings,
                new MockStoreFunctions(),
                (allocatorSettings, storeFunctions) => new MockAllocator());

            // Act
            tsavoriteKV.logger = loggerMock.Object;

            // Assert
            loggerMock.Verify(l => l.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"), Times.Once);
        }
    }

    public class MockStoreFunctions : IStoreFunctions<string, string>
    {
        public long GetKeyHashCode64(ref string key)
        {
            return 0;
        }

        public bool KeysEqual(ref string key1, ref string key2)
        {
            return true;
        }

        public IObj<Stream> BeginSerializeKey(Stream stream)
        {
            return null;
        }

        public IObj<Stream> BeginDeserializeKey(Stream stream)
        {
            return null;
        }

        public IObj<Stream> BeginSerializeValue(Stream stream)
        {
            return null;
        }

        public IObj<Stream> BeginDeserializeValue(Stream stream)
        {
            return null;
        }

        public void DisposeRecord(ref string key, ref string value, DisposeReason reason, int sessionID)
        {
        }

        public void SetCheckpointCompletedCallback(Action callback)
        {
        }
    }

    public class MockAllocator : IAllocator<string, string, MockStoreFunctions>
    {
        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public AllocatorBase<string, string, MockStoreFunctions, MockAllocator> GetBase<MockAllocator>()
        {
            return null;
        }

        public void GetAndInitializeValue(long address, long sessionID)
        {
        }

        public int GetRMWCopyDestinationRecordSize<TInput, TVariableLengthInput>(ref string key, ref TInput input, ref string value, ref RecordInfo recordInfo, TVariableLengthInput variableLengthInput)
        {
            return 0;
        }

        public int GetRMWInitialRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            return 0;
        }

        public int GetUpsertRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref string value, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            return 0;
        }

        public int GetRecordSize(ref string key, ref string value)
        {
            return 0;
        }
    }
}
