using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

public class TsavoriteKVTests
{
    [Fact]
    public void Constructor_LogsInformation_WhenCheckpointManagerAndCheckpointDirSpecified()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var kvSettings = new KVSettings<object, object>
        {
            logger = mockLogger.Object,
            loggerFactory = mockLoggerFactory.Object,
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
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
