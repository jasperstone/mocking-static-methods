using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

public class TsavoriteKVTests
{
    [Fact]
    public void LogInformation_ShouldLogCorrectMessage_WhenCheckpointManagerAndCheckpointDirSpecified()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var kvSettings = new KVSettings<object, object>
        {
            loggerFactory = loggerFactoryMock.Object
        };

        kvSettings.SetCheckpointSettings(new CheckpointSettings
        {
            CheckpointDir = "some/dir",
            CheckpointManager = new object() // Any non-null value
        });

        var storeFunctions = new Mock<IStoreFunctions<object, object>>().Object;
        var allocatorFactory = (AllocatorSettings settings, IStoreFunctions<object, object> storeFuncs) => new Mock<IAllocator<object, object, IStoreFunctions<object, object>>>().Object;

        // Act
        var tsavoriteKV = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(
            kvSettings, storeFunctions, allocatorFactory);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"),
            Times.Once);
    }
}
