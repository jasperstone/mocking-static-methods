using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
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
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ulong GetKeyHashCode64(ref string key)
        {
            throw new NotImplementedException();
        }

        public bool KeysEqual(ref string key1, ref string key2)
        {
            throw new NotImplementedException();
        }

        public IObj BeginSerializeKey(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IObj BeginDeserializeKey(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IObj BeginSerializeValue(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IObj BeginDeserializeValue(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void DisposeRecord(ref string key, ref string value, DisposeReason reason, int sessionID)
        {
            throw new NotImplementedException();
        }
    }

    public class MockAllocator : IAllocator<string, string, MockStoreFunctions>
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public AllocatorBase<string, string, MockStoreFunctions, MockAllocator> GetBase<MockAllocator>()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public long GetAndInitializeValue(long address, long sessionID)
        {
            throw new NotImplementedException();
        }

        public long GetRMWCopyDestinationRecordSize<TInput, TVariableLengthInput>(ref string key, ref TInput input, ref string value, ref RecordInfo recordInfo, TVariableLengthInput variableLengthInput)
        {
            throw new NotImplementedException();
        }

        public long GetRMWInitialRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            throw new NotImplementedException();
        }

        public long GetUpsertRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref string value, ref TInput input, TSessionFunctionsWrapper sessionFunctionsWrapper)
        {
            throw new NotImplementedException();
        }
    }
}
