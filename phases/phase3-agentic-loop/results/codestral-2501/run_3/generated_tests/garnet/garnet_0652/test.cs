using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace TsavoriteTests
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
                GetCheckpointSettings = () => new CheckpointSettings
                {
                    CheckpointDir = "testDir",
                    CheckpointManager = new Mock<ICheckpointManager>().Object
                }
            };

            var storeFunctions = new Mock<IStoreFunctions<object, object>>().Object;
            var allocatorFactory = new Func<AllocatorSettings, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>
                ((allocatorSettings, functions) => new Mock<IAllocator<object, object, IStoreFunctions<object, object>>>().Object);

            // Act
            var tsavoriteKV = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(kvSettings, storeFunctions, allocatorFactory);

            // Assert
            mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}

public class KVSettings<TKey, TValue>
{
    public ILogger logger { get; set; }
    public Func<CheckpointSettings> GetCheckpointSettings { get; set; }
}

public class CheckpointSettings
{
    public string CheckpointDir { get; set; }
    public ICheckpointManager CheckpointManager { get; set; }
}

public interface ICheckpointManager { }

public interface IStoreFunctions<TKey, TValue> { }

public interface IAllocator<TKey, TValue, TStoreFunctions> where TStoreFunctions : IStoreFunctions<TKey, TValue> { }

public class AllocatorSettings { }

public class TsavoriteBase { }

public class TsavoriteKV<TKey, TValue, TStoreFunctions, TAllocator> : TsavoriteBase
    where TStoreFunctions : IStoreFunctions<TKey, TValue>
    where TAllocator : IAllocator<TKey, TValue, TStoreFunctions>
{
    public TsavoriteKV(KVSettings<TKey, TValue> kvSettings, TStoreFunctions storeFunctions, Func<AllocatorSettings, TStoreFunctions, TAllocator> allocatorFactory)
    {
        var checkpointSettings = kvSettings.GetCheckpointSettings() ?? new CheckpointSettings();

        if (checkpointSettings.CheckpointDir != null && checkpointSettings.CheckpointManager != null)
        {
            kvSettings.logger?.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir");
        }
    }
}
