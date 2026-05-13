using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void LogInformation_Called_When_CheckpointManager_And_CheckpointDir_Specified()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var kvSettings = new KVSettings<object, object>
            {
                logger = mockLogger.Object,
                CheckpointSettings = new CheckpointSettings
                {
                    CheckpointDir = "testDir",
                    CheckpointManager = new Mock<ICheckpointManager>().Object
                }
            };
            var storeFunctions = new Mock<IStoreFunctions<object, object>>().Object;
            var allocatorFactory = new Mock<Func<AllocatorSettings, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>>().Object;

            // Act
            var tsavoriteKV = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(kvSettings, storeFunctions, allocatorFactory);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class KVSettings<TKey, TValue>
    {
        public LightEpoch Epoch { get; set; }
        public ILogger Logger { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public CheckpointSettings CheckpointSettings { get; set; }
        public bool ReadCacheEnabled { get; set; }
        public LogSettings GetLogSettings() => new LogSettings();
    }

    public class CheckpointSettings
    {
        public string CheckpointDir { get; set; }
        public ICheckpointManager CheckpointManager { get; set; }
        public bool RemoveOutdated { get; set; }
        public int ThrottleCheckpointFlushDelayMs { get; set; }
    }

    public interface ICheckpointManager { }

    public class LogSettings
    {
        public ReadCopyOptions ReadCopyOptions { get; set; }
    }

    public class ReadCopyOptions
    {
        public ReadCopyTo CopyTo { get; set; }
        public ReadCopyFrom CopyFrom { get; set; }
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

    public class LightEpoch { }

    public interface IStoreFunctions<TKey, TValue> { }

    public interface IAllocator<TKey, TValue, TStoreFunctions> { }

    public class AllocatorSettings
    {
        public AllocatorSettings(LogSettings logSettings, LightEpoch epoch, ILogger logger) { }
    }
}
