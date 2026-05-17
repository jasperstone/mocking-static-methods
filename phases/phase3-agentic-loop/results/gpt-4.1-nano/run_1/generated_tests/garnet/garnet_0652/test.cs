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
            var loggerMock = new Mock<ILogger<TsavoriteKV<int, string, MockStoreFunctions, MockAllocator>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var checkpointSettings = new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new object(), // just to simulate non-null
                RemoveOutdated = false
            };

            var kvSettingsMock = new Mock<KVSettings<int, string>>();
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(checkpointSettings);
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new LogSettings());
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(false);
            kvSettingsMock.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettingsMock.Setup(s => s.logger).Returns((ILogger)null);
            kvSettingsMock.Setup(s => s.Epoch).Returns(0);

            var storeFunctions = new MockStoreFunctions();
            Func<AllocatorSettings, MockStoreFunctions, MockAllocator> allocatorFactory = (settings, store) => new MockAllocator();

            // Act
            var kv = new TsavoriteKV<int, string, MockStoreFunctions, MockAllocator>(kvSettingsMock.Object, storeFunctions, allocatorFactory);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock classes for dependencies
    public class MockStoreFunctions : IStoreFunctions<int, string> { }
    public class MockAllocator : IAllocator<int, string, MockStoreFunctions> 
    {
        public void Initialize() { }
        public IAllocator<int, string, MockStoreFunctions> GetBase<T>() => this;
    }
    public class CheckpointSettings
    {
        public string CheckpointDir { get; set; }
        public object CheckpointManager { get; set; }
        public bool RemoveOutdated { get; set; }
        public int ThrottleCheckpointFlushDelayMs { get; set; }
    }
    public class LogSettings
    {
        public ReadCopyOptions ReadCopyOptions { get; set; } = new();
        public ReadCacheSettings ReadCacheSettings { get; set; } = new();
    }
    public class ReadCopyOptions
    {
        public ReadCopyTo CopyTo { get; set; } = ReadCopyTo.Inherit;
        public ReadCopyFrom CopyFrom { get; set; } = ReadCopyFrom.Inherit;
    }
    public class ReadCacheSettings
    {
        public int PageSizeBits { get; set; }
        public int MemorySizeBits { get; set; }
        public double SecondChanceFraction { get; set; }
    }
    public enum ReadCopyTo { Inherit, ReadCache, None }
    public enum ReadCopyFrom { Inherit, Device }
    public class KVSettings<TKey, TValue>
    {
        public int Epoch { get; set; }
        public ILogger logger { get; set; }
        public ILoggerFactory loggerFactory { get; set; }
        public bool ReadCacheEnabled { get; set; }
        public CheckpointSettings GetCheckpointSettings() => new();
        public LogSettings GetLogSettings() => new();
    }
}
