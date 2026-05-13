using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Tsavorite.core
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
            tsavoriteKV = new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
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

        public string Get(string key)
        {
            throw new NotImplementedException();
        }

        public void Put(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
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

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
