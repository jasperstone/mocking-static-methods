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
        // Minimal stub for KVSettings to allow construction
        public class TestKVSettings<TKey, TValue> : KVSettings<TKey, TValue>
        {
            public TestKVSettings(ILogger logger, ILoggerFactory loggerFactory, string checkpointDir, ICheckpointManager checkpointManager)
            {
                this.logger = logger;
                this.loggerFactory = loggerFactory;
                this.CheckpointDir = checkpointDir;
                this.CheckpointManager = checkpointManager;
            }

            public override string CheckpointDir { get; }
            public override ICheckpointManager CheckpointManager { get; }

            public override CheckpointSettings GetCheckpointSettings()
            {
                return new CheckpointSettings
                {
                    CheckpointDir = CheckpointDir,
                    CheckpointManager = CheckpointManager,
                    ThrottleCheckpointFlushDelayMs = 0,
                    RemoveOutdated = false
                };
            }

            public override LogSettings GetLogSettings()
            {
                return new LogSettings
                {
                    ReadCopyOptions = new ReadCopyOptions()
                };
            }

            public override bool ReadCacheEnabled => false;

            public override IEpoch Epoch => new TestEpoch();

            // Other members can be default or throw NotImplementedException
        }

        // Minimal stub for ICheckpointManager
        public interface ICheckpointManager { }

        // Minimal stub for CheckpointSettings
        public class CheckpointSettings
        {
            public string CheckpointDir { get; set; }
            public ICheckpointManager CheckpointManager { get; set; }
            public int ThrottleCheckpointFlushDelayMs { get; set; }
            public bool RemoveOutdated { get; set; }
        }

        // Minimal stub for LogSettings
        public class LogSettings
        {
            public ReadCopyOptions ReadCopyOptions { get; set; }
        }

        // Minimal stub for ReadCopyOptions
        public class ReadCopyOptions
        {
            public ReadCopyTo CopyTo { get; set; } = ReadCopyTo.Inherit;
            public ReadCopyFrom CopyFrom { get; set; } = ReadCopyFrom.Inherit;
        }

        public enum ReadCopyTo
        {
            Inherit,
            ReadCache,
            None
        }

        public enum ReadCopyFrom
        {
            Inherit,
            Device
        }

        // Minimal stub for IEpoch
        public interface IEpoch
        {
            void Resume();
            void Suspend();
            void BumpCurrentEpoch(Action callback);
        }

        public class TestEpoch : IEpoch
        {
            public void Resume() { }
            public void Suspend() { }
            public void BumpCurrentEpoch(Action callback) => callback();
        }

        // Minimal stub for TStoreFunctions
        public interface IStoreFunctions<TKey, TValue> { }

        // Minimal stub for TAllocator
        public interface IAllocator<TKey, TValue, TStoreFunctions> { }

        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointDirAndManagerSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var checkpointManagerMock = new Mock<ICheckpointManager>();

            var kvSettings = new TestKVSettings<string, string>(
                loggerMock.Object,
                loggerFactoryMock.Object,
                checkpointDir: "someDir",
                checkpointManager: checkpointManagerMock.Object);

            var storeFunctionsMock = new Mock<IStoreFunctions<string, string>>();

            Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>> allocatorFactory =
                (settings, storeFuncs) => new Mock<IAllocator<string, string, IStoreFunctions<string, string>>>().Object;

            // Act
            var tsavorite = new TsavoriteKV<string, string, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>>(
                kvSettings,
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
