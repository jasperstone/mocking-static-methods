using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

public class TsavoriteKVTests
{
    [Fact]
    public void LogInformation_Called_When_CheckpointManager_And_CheckpointDir_Specified()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var kvSettings = new KVSettings<int, string>
        {
            logger = mockLogger.Object,
            GetCheckpointSettings = () => new CheckpointSettings
            {
                CheckpointDir = "testDir",
                CheckpointManager = new Mock<ICheckpointManager>().Object
            }
        };

        var storeFunctions = new Mock<IStoreFunctions<int, string>>().Object;
        var allocatorFactory = new Mock<Func<AllocatorSettings, IStoreFunctions<int, string>, IAllocator<int, string, IStoreFunctions<int, string>>>>().Object;

        // Act
        var tsavoriteKV = new TsavoriteKV<int, string, IStoreFunctions<int, string>, IAllocator<int, string, IStoreFunctions<int, string>>>(kvSettings, storeFunctions, allocatorFactory);

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
    public ILogger logger { get; set; }
    public ILoggerFactory loggerFactory { get; set; }
    public Func<CheckpointSettings> GetCheckpointSettings { get; set; }
    public bool ReadCacheEnabled { get; set; }
    public LogSettings GetLogSettings() => new LogSettings();
}

public class CheckpointSettings
{
    public string CheckpointDir { get; set; }
    public ICheckpointManager CheckpointManager { get; set; }
    public bool RemoveOutdated { get; set; }
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

public interface IStoreFunctions<TKey, TValue> { }

public interface IAllocator<TKey, TValue, TStoreFunctions> where TStoreFunctions : IStoreFunctions<TKey, TValue>
{
    AllocatorBase<TKey, TValue, TStoreFunctions, IAllocator<TKey, TValue, TStoreFunctions>> GetBase<TAllocator>();
    bool IsFixedLength { get; }
    bool HasObjectLog { get; }
}

public class AllocatorBase<TKey, TValue, TStoreFunctions, TAllocator> where TStoreFunctions : IStoreFunctions<TKey, TValue> where TAllocator : IAllocator<TKey, TValue, TStoreFunctions>
{
    public void Initialize() { }
    public long MaxAllocatedPageCount { get; }
}

public class AllocatorSettings
{
    public LogSettings LogSettings { get; set; }
    public LightEpoch epoch { get; set; }
    public ILogger logger { get; set; }
    public Action<long, long> evictCallback { get; set; }

    public AllocatorSettings(LogSettings logSettings, LightEpoch epoch, ILogger logger)
    {
        LogSettings = logSettings;
        this.epoch = epoch;
        this.logger = logger;
    }
}

public class LightEpoch
{
    public void Resume() { }
    public void Suspend() { }
    public void BumpCurrentEpoch(Action action) { }
    public void Dispose() { }
}
