using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    // Dummy implementations for generic constraints
    public class DummyStoreFunctions : IStoreFunctions<int, int> { }
    public class DummyAllocator : IAllocator<int, int, DummyStoreFunctions>
    {
        public AllocatorBase<int, int, DummyStoreFunctions, DummyAllocator> GetBase<T>() where T : IAllocator<int, int, DummyStoreFunctions> => null!;
        public bool HasObjectLog => false;
        public bool IsFixedLength => true;
        public void Dispose() { }
    }

    public class TsavoriteKVTests
    {
        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointDirAndCheckpointManagerSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var checkpointManagerMock = new Mock<ICheckpointManager>();

            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = checkpointManagerMock.Object,
                RemoveOutdated = false,
                ThrottleCheckpointFlushDelayMs = 123
            };

            var kvSettingsMock = new Mock<KVSettings<int, int>>();
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(checkpointSettings);
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new LogSettings());
            kvSettingsMock.SetupGet(s => s.ReadCacheEnabled).Returns(false);
            kvSettingsMock.SetupGet(s => s.Epoch).Returns(new Epoch());
            kvSettingsMock.SetupGet(s => s.logger).Returns((ILogger)null);
            kvSettingsMock.SetupGet(s => s.loggerFactory).Returns(loggerFactoryMock.Object);

            var storeFunctions = new DummyStoreFunctions();

            Func<AllocatorSettings, DummyStoreFunctions, DummyAllocator> allocatorFactory = (settings, store) =>
            {
                return new DummyAllocator();
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
