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
        // Minimal dummy implementations to satisfy generic constraints and dependencies
        public class DummyStoreFunctions : IStoreFunctions<int, int>
        {
            public long GetKeyHashCode64(ref int key) => key.GetHashCode();
            public bool KeysEqual(ref int k1, ref int k2) => k1 == k2;
            public bool HasKeySerializer => false;
            public IObjectSerializer<int> BeginSerializeKey(Stream stream) => null;
            public IObjectSerializer<int> BeginDeserializeKey(Stream stream) => null;
            public bool HasValueSerializer => false;
            public IObjectSerializer<int> BeginSerializeValue(Stream stream) => null;
            public IObjectSerializer<int> BeginDeserializeValue(Stream stream) => null;
            public bool DisposeOnPageEviction => false;
            public void DisposeRecord(ref int key, ref int value, DisposeReason reason, int newKeySize = -1) { }
            public void SetCheckpointCompletedCallback(Action callback) { }
            public void OnCheckpointCompleted() { }
        }

        public class DummyAllocator : IAllocator<int, int, DummyStoreFunctions>
        {
            public AllocatorBase<int, int, DummyStoreFunctions, DummyAllocator> GetBase<T>() where T : IAllocator<int, int, DummyStoreFunctions> => null;
            public bool IsFixedLength => true;
            public bool HasObjectLog => false;
        }

        // Minimal stubs for CheckpointSettings and LogSettings to allow construction
        public class DummyCheckpointSettings
        {
            public string CheckpointDir { get; set; }
            public object CheckpointManager { get; set; }
            public bool RemoveOutdated { get; set; }
            public int ThrottleCheckpointFlushDelayMs { get; set; }
        }

        public class DummyLogSettings
        {
            public ReadCopyOptions ReadCopyOptions { get; set; } = new ReadCopyOptions();
            public ReadCacheSettings ReadCacheSettings { get; set; } = new ReadCacheSettings();
        }

        // Minimal dummy KVSettings class with required members
        public class DummyKVSettings : KVSettings<int, int>
        {
            public ILogger logger { get; set; }
            public ILoggerFactory loggerFactory { get; set; }
            public DummyCheckpointSettings checkpointSettings { get; set; }
            public DummyLogSettings logSettings { get; set; }
            public bool ReadCacheEnabled { get; set; }
            public int Epoch { get; set; }

            public override CheckpointSettings GetCheckpointSettings()
            {
                // Map dummy to real CheckpointSettings for constructor
                var cs = new CheckpointSettings();
                cs.CheckpointDir = checkpointSettings.CheckpointDir;
                cs.CheckpointManager = checkpointSettings.CheckpointManager;
                cs.RemoveOutdated = checkpointSettings.RemoveOutdated;
                cs.ThrottleCheckpointFlushDelayMs = checkpointSettings.ThrottleCheckpointFlushDelayMs;
                return cs;
            }

            public override LogSettings GetLogSettings()
            {
                // Map dummy to real LogSettings for constructor
                var ls = new LogSettings();
                ls.ReadCopyOptions = logSettings.ReadCopyOptions;
                ls.ReadCacheSettings = logSettings.ReadCacheSettings;
                return ls;
            }
        }

        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointManagerAndCheckpointDirSpecified()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var checkpointManager = new object(); // dummy non-null checkpoint manager

            var kvSettings = new DummyKVSettings
            {
                logger = null,
                loggerFactory = mockLoggerFactory.Object,
                checkpointSettings = new DummyCheckpointSettings
                {
                    CheckpointDir = "someDir",
                    CheckpointManager = checkpointManager,
                    RemoveOutdated = false,
                    ThrottleCheckpointFlushDelayMs = 100
                },
                logSettings = new DummyLogSettings(),
                ReadCacheEnabled = false,
                Epoch = 0
            };

            var storeFunctions = new DummyStoreFunctions();
            Func<AllocatorSettings, DummyStoreFunctions, DummyAllocator> allocatorFactory = (settings, funcs) =>
            {
                return new DummyAllocator();
            };

            // Act
            var tsavorite = new TsavoriteKV<int, int, DummyStoreFunctions, DummyAllocator>(kvSettings, storeFunctions, allocatorFactory);

            // Assert
            mockLogger.Verify(
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
