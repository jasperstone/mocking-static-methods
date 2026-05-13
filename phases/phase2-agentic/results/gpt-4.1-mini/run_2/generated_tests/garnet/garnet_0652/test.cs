using System;
using System.IO;
using System.Threading;
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
            public int GetHashCode(int key) => key.GetHashCode();
            public bool Equals(int key1, int key2) => key1 == key2;
            public int Size => 4;
        }

        private class DummyAllocator : IAllocator<int, int, DummyStoreFunctions>
        {
            public bool IsFixedLength => true;
            public bool HasObjectLog => false;
            public AllocatorBase<int, int, DummyStoreFunctions, DummyAllocator> GetBase<T>() where T : IAllocator<int, int, DummyStoreFunctions> => new DummyAllocatorBase();
            public void Dispose() { }
        }

        private class DummyAllocatorBase : AllocatorBase<int, int, DummyStoreFunctions, DummyAllocator>
        {
            public override void Initialize() { }
            public override long MaxAllocatedPageCount => 0;
        }

        private class DummyKVSettings : KVSettings<int, int>
        {
            public override CheckpointSettings GetCheckpointSettings() => new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new Mock<ICheckpointManager>().Object,
                RemoveOutdated = true,
                ThrottleCheckpointFlushDelayMs = 123
            };

            public override LogSettings GetLogSettings() => new LogSettings
            {
                ReadCopyOptions = new ReadCopyOptions
                {
                    CopyTo = ReadCopyTo.Inherit,
                    CopyFrom = ReadCopyFrom.Inherit
                },
                ReadCacheSettings = new ReadCacheSettings
                {
                    PageSizeBits = 12,
                    MemorySizeBits = 20,
                    SecondChanceFraction = 0.2f
                }
            };

            public override bool ReadCacheEnabled => true;

            public override ILogger Logger => null;

            public override ILoggerFactory LoggerFactory => new LoggerFactory();
        }

        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointManagerAndDirSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettingsMock = new Mock<KVSettings<int, int>>();
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new Mock<ICheckpointManager>().Object,
                RemoveOutdated = true,
                ThrottleCheckpointFlushDelayMs = 123
            });
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new LogSettings
            {
                ReadCopyOptions = new ReadCopyOptions
                {
                    CopyTo = ReadCopyTo.Inherit,
                    CopyFrom = ReadCopyFrom.Inherit
                },
                ReadCacheSettings = new ReadCacheSettings
                {
                    PageSizeBits = 12,
                    MemorySizeBits = 20,
                    SecondChanceFraction = 0.2f
                }
            });
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(true);
            kvSettingsMock.Setup(s => s.Logger).Returns((ILogger)null);
            kvSettingsMock.Setup(s => s.LoggerFactory).Returns(loggerFactoryMock.Object);
            kvSettingsMock.Setup(s => s.Epoch).Returns(new Epoch());

            var storeFunctions = new DummyStoreFunctions();

            // Act
            var tsavorite = new TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>(
                kvSettingsMock.Object,
                storeFunctions,
                (settings, funcs) => new DummyAllocator());

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
