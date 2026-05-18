using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKvTests
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
            };

            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = "/path/to/checkpoint/dir",
                CheckpointManager = new DeviceLogCommitCheckpointManager(
                    new LocalStorageNamedDeviceFactoryCreator(),
                    new DefaultCheckpointNamingScheme(
                        new DirectoryInfo("/path/to/checkpoint/dir").FullName),
                    removeOutdated: false)
            };

            // Act
            var tsavoriteKv = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(kvSettings, new Mock<IStoreFunctions<object, object>>().Object, (settings, functions) => new Mock<IAllocator<object, object, IStoreFunctions<object, object>>>().Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"), Times.Once);
        }
    }
}
