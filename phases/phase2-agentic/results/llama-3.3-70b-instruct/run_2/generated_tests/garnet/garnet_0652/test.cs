using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Tsavorite.core
{
    public class TsavoriteTests
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

            // Act
            var tsavorite = new TsavoriteKV<string, string, StoreFunctions, Allocator>(
                kvSettings,
                new StoreFunctions(),
                (allocatorSettings, storeFunctions) => new Allocator(allocatorSettings, storeFunctions));

            // Assert
            loggerMock.Verify(l => l.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"), Times.Once);
        }

        [Fact]
        public void LogInformation_NotCalled_When_CheckpointManager_Not_Specified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kvSettings = new KVSettings<string, string>
            {
                CheckpointSettings = new CheckpointSettings
                {
                    CheckpointDir = "/path/to/checkpoint/dir"
                }
            };

            // Act
            var tsavorite = new TsavoriteKV<string, string, StoreFunctions, Allocator>(
                kvSettings,
                new StoreFunctions(),
                (allocatorSettings, storeFunctions) => new Allocator(allocatorSettings, storeFunctions));

            // Assert
            loggerMock.Verify(l => l.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"), Times.Never);
        }

        private class StoreFunctions : IStoreFunctions<string, string>
        {
            public void Add(string key, string value)
            {
                throw new NotImplementedException();
            }

            public void Remove(string key)
            {
                throw new NotImplementedException();
            }

            public string Get(string key)
            {
                throw new NotImplementedException();
            }
        }

        private class Allocator : IAllocator<string, string, StoreFunctions>
        {
            public Allocator(AllocatorSettings settings, StoreFunctions storeFunctions)
            {
                throw new NotImplementedException();
            }

            public void Add(string key, string value)
            {
                throw new NotImplementedException();
            }

            public void Remove(string key)
            {
                throw new NotImplementedException();
            }

            public string Get(string key)
            {
                throw new NotImplementedException();
            }
        }
    }
}
