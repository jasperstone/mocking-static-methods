using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteLoggingTests
    {
        [Fact]
        public void LogInformation_IsCalled_WhenCheckpointManagerAndDirAreSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettingsMock = new Mock<IVar>();
            // Since the actual type is not accessible, we will create a minimal mock class for testing
            // but for now, assume we can mock the methods and properties used
            // For the purpose of this test, we will create a dummy class with the needed properties

            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new object(),
                RemoveOutdated = false
            };

            var getCheckpointSettingsCalled = false;
            var getLogSettingsCalled = false;

            var kvSettings = new Mock<KVSettings<int, string>>();
            kvSettings.Setup(s => s.GetCheckpointSettings()).Returns(checkpointSettings);
            kvSettings.Setup(s => s.logger).Returns((ILogger)null);
            kvSettings.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettings.Setup(s => s.GetLogSettings()).Returns(new LogSettings
            {
                ReadCopyOptions = new ReadCopyOptions { CopyTo = ReadCopyTo.Inherit, CopyFrom = ReadCopyFrom.Inherit }
            });
            kvSettings.Setup(s => s.ReadCacheEnabled).Returns(false);
            kvSettings.Setup(s => s.Epoch).Returns(1);

            var storeFunctionsMock = new Mock<IStoreFunctions<int, string>>();
            var allocatorFactoryMock = new Func<AllocatorSettings, IStoreFunctions<int, string>, IAllocator<int, string>>(
                (settings, store) => Mock.Of<IAllocator<int, string>>());

            // Act
            var kv = new TsavoriteKV<int, string, IStoreFunctions<int, string>, IAllocator<int, string>>(
                kvSettings.Object, storeFunctionsMock.Object, allocatorFactoryMock);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"),
                Times.Once);
        }
    }
}
