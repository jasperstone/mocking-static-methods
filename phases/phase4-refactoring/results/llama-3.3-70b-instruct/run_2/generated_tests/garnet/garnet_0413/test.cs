using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet.common;
using Tsavorite.core;

public class SingleDatabaseManagerTests
{
    [Fact]
    public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogInformationCalled_WhenAofSizeLimitExceeded()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var singleDatabaseManager = new SingleDatabaseManager(null, storeWrapperMock.Object, false);
        singleDatabaseManager.DefaultDatabase = new GarnetDatabase(0, null, true);
        singleDatabaseManager.DefaultDatabase.AppendOnlyFile = new AppendOnlyFile { TailAddress = 100, BeginAddress = 0 };

        // Act
        await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(50, logger: loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
}
