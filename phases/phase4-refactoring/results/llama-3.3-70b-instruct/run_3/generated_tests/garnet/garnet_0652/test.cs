using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void LogInformation_Called_When_CheckpointManager_And_CheckpointDir_Specified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kvSettings = new KVSettings<object, object>
            {
                logger = loggerMock.Object,
                loggerFactory = null,
                CheckpointSettings = new CheckpointSettings
                {
                    CheckpointDir = "/path/to/checkpoint/dir",
                    CheckpointManager = new DeviceLogCommitCheckpointManager(
                        new LocalStorageNamedDeviceFactoryCreator(),
                        new DefaultCheckpointNamingScheme(
                            new DirectoryInfo("/path/to/checkpoint/dir").FullName),
                        removeOutdated: false)
                }
            };

            var storeFunctions = new Mock<IStoreFunctions<object, object>>().Object;
            var allocatorFactory = new Func<AllocatorSettings, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>((allocatorSettings, storeFunctions) => null);

            // Act
            var tsavoriteKV = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(kvSettings, storeFunctions, allocatorFactory);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"), Times.Once);
        }
    }
}
