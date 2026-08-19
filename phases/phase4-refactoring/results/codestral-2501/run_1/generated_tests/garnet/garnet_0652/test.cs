using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

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
            CheckpointSettings = new CheckpointSettings
            {
                CheckpointDir = "someDir",
                CheckpointManager = new Mock<ICheckpointManager>().Object
            }
        };
        var storeFunctions = new Mock<IStoreFunctions<int, string>>().Object;
        var allocatorFactory = new Mock<Func<AllocatorSettings, IStoreFunctions<int, string>, IAllocator<int, string, IStoreFunctions<int, string>>>>().Object;

        // Act
        var tsavoriteKV = new TsavoriteKV<int, string, IStoreFunctions<int, string>, IAllocator<int, string, IStoreFunctions<int, string>>>(kvSettings, storeFunctions, allocatorFactory);

        // Assert
        mockLogger.Verify(logger => logger.Log(
            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
