using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsavorite.core;
using Xunit;

namespace TsavoriteTests
{
    public class TsavoriteTests
    {
        [Fact]
        public void TestLogInformation()
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

            // Act
            var tsavorite = new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
                kvSettings,
                new MockStoreFunctions(),
                (allocatorSettings, storeFunctions) => new MockAllocator());

            // Assert
            loggerMock.Verify(l => l.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"), Times.Once);
        }
    }

    public class MockStoreFunctions : IStoreFunctions<string, string>
    {
        public long GetKeyHashCode64(ref string key)
        {
            return key.GetHashCode();
        }

        public bool KeysEqual(ref string key1, ref string key2)
        {
            return key1 == key2;
        }

        public void BeginSerializeKey(Stream stream, ref string key)
        {
            using var writer = new StreamWriter(stream);
            writer.Write(key);
        }

        public void BeginDeserializeKey(Stream stream, ref string key)
        {
            using var reader = new StreamReader(stream);
            key = reader.ReadToEnd();
        }

        public void BeginSerializeValue(Stream stream, ref string value)
        {
            using var writer = new StreamWriter(stream);
            writer.Write(value);
        }

        public void BeginDeserializeValue(Stream stream, ref string value)
        {
            using var reader = new StreamReader(stream);
            value = reader.ReadToEnd();
        }

        public string Get(string key)
        {
            return key;
        }

        public void Put(string key, string value)
        {
        }

        public void Remove(string key)
        {
        }

        public void Dispose()
        {
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
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public AllocatorBase<string, string, MockStoreFunctions, MockAllocator> GetBase<MockAllocator>()
        {
            throw new NotImplementedException();
        }

        public (long, long) GetAndInitializeValue(long address, long sessionID)
        {
            throw new NotImplementedException();
        }

        public int GetRMWCopyDestinationRecordSize<TInput, TVariableLengthInput>(ref string key, ref TInput input, ref string value, ref RecordInfo recordInfo, TVariableLengthInput variableLengthInput)
        {
            throw new NotImplementedException();
        }

        public int GetRMWInitialRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            throw new NotImplementedException();
        }

        public int GetUpsertRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref string value, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            throw new NotImplementedException();
        }

        public int GetRecordSize(ref string key, ref string value)
        {
            throw new NotImplementedException();
        }
    }
}
