using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteKVTests
    {
        private class DummyStoreFunctions : IStoreFunctions<int, int>
        {
            // Implement interface members with dummy behavior if needed
        }

        private class DummyAllocator : IAllocator<int, int, DummyStoreFunctions>
        {
            public bool IsFixedLength => true;
            public AllocatorBase<int, int, DummyStoreFunctions, DummyAllocator> GetBase<T>() where T : IAllocator<int, int, DummyStoreFunctions> => null;
            public bool HasObjectLog => false;
            public void Initialize() { }
        }

        private class DummyKVSettings : KVSettings<int, int>
        {
            public override CheckpointSettings GetCheckpointSettings() => new CheckpointSettings();
            public override LogSettings GetLogSettings() => new LogSettings();
        }

        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointManagerAndDirSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettingsMock = new Mock<KVSettings<int, int>>();
            kvSettingsMock.Setup(k => k.GetCheckpointSettings()).Returns(new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new Mock<ICheckpointManager>().Object
            });
            kvSettingsMock.Setup(k => k.logger).Returns((ILogger)null);
            kvSettingsMock.Setup(k => k.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettingsMock.Setup(k => k.Epoch).Returns(new Epoch());

            var storeFunctions = new DummyStoreFunctions();

            Func<AllocatorSettings, DummyStoreFunctions, DummyAllocator> allocatorFactory = (settings, store) =>
            {
                var allocator = new DummyAllocator();
                return allocator;
            };

            // Act
            var tsavorite = new TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>(
                kvSettingsMock.Object,
                storeFunctions,
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
