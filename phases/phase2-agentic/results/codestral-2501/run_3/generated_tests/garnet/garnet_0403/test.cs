using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Threading.Tasks;

public class MultiDatabaseManagerTests
{
    [Fact]
    public void RecoverCheckpoint_LogsInformationOnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MultiDatabaseManager>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServerOptions = new Mock<GarnetServerOptions>();
        var mockDatabaseManager = new Mock<DatabaseManagerBase>(MockBehavior.Strict, null, null);

        mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(mockLogger.Object);
        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);

        var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), mockStoreWrapper.Object);

        var exception = new Exception("Test exception");

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of database ids")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_ThrowsOnFailOnRecoveryError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MultiDatabaseManager>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServerOptions = new Mock<GarnetServerOptions>();
        var mockDatabaseManager = new Mock<DatabaseManagerBase>(MockBehavior.Strict, null, null);

        mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(mockLogger.Object);
        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockServerOptions.Setup(so => so.FailOnRecoveryError).Returns(true);

        var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), mockStoreWrapper.Object);

        var exception = new Exception("Test exception");

        // Act & Assert
        Assert.Throws<GarnetException>(() => multiDatabaseManager.RecoverCheckpoint());
    }

    [Fact]
    public async Task TakeCheckpointAsync_ReturnsFalseIfLockNotAcquired()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MultiDatabaseManager>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServerOptions = new Mock<GarnetServerOptions>();
        var mockDatabaseManager = new Mock<DatabaseManagerBase>(MockBehavior.Strict, null, null);

        mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(mockLogger.Object);
        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);

        var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), mockStoreWrapper.Object);

        // Act
        var result = await multiDatabaseManager.TakeCheckpointAsync(false);

        // Assert
        Assert.False(result);
    }
}
