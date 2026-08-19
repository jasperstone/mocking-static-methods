using System;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointManagerAndDirSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettingsMock = new Mock<KVSettings<int, int>>();
            kvSettingsMock.Setup(s => s.Epoch).Returns(0);
            kvSettingsMock.Setup(s => s.logger).Returns((ILogger)null);
            kvSettingsMock.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);

            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new DeviceLogCommitCheckpointManager(
                    new LocalStorageNamedDeviceFactoryCreator(),
                    new DefaultCheckpointNamingScheme("someDir"),
                    removeOutdated: false)
            };
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(checkpointSettings);
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new LogSettings());
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(false);

            var storeFunctionsMock = new Mock<IStoreFunctions<int, int>>();

            Func<AllocatorSettings, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>> allocatorFactory = (settings, funcs) =>
            {
                var allocatorMock = new Mock<IAllocator<int, int, IStoreFunctions<int, int>>>();
                allocatorMock.Setup(a => a.IsFixedLength).Returns(true);
                allocatorMock.Setup(a => a.GetBase<IAllocator<int, int, IStoreFunctions<int, int>>>()).Returns((AllocatorBase<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>)null);
                allocatorMock.Setup(a => a.HasObjectLog).Returns(false);
                allocatorMock.Setup(a => a.Initialize());
                return allocatorMock.Object;
            };

            // Act
            var tsavorite = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>(
                kvSettingsMock.Object,
                storeFunctionsMock.Object,
                allocatorFactory);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
