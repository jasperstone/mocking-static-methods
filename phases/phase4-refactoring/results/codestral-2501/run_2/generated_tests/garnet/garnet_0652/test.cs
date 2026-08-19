using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System;
using System.IO;

public class TsavoriteKVTests
{
    [Fact]
    public void LogInformation_Called_When_CheckpointManager_And_CheckpointDir_Specified()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var kvSettings = new KVSettings<string, string>
        {
            logger = mockLogger.Object,
            CheckpointDir = "testDir",
            CheckpointManager = Mock.Of<ICheckpointManager>()
        };

        var storeFunctions = Mock.Of<IStoreFunctions<string, string>>();
        var allocatorFactory = new Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>>((settings, functions) => Mock.Of<IAllocator<string, string, IStoreFunctions<string, string>>>());

        // Act
        var tsavoriteKV = new TsavoriteKV<string, string, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>>>(kvSettings, storeFunctions, allocatorFactory);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
