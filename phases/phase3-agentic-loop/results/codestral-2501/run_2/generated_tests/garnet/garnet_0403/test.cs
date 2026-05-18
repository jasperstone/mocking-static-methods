using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MultiDatabaseManagerTests
{
    [Fact]
    public void LogInformation_Called_When_RecoverDatabaseCheckpoint_Throws_Exception()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MultiDatabaseManager>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServerOptions = new Mock<GarnetServerOptions>();
        var mockDatabaseManager = new Mock<DatabaseManagerBase>(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), mockStoreWrapper.Object);

        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());

        var multiDatabaseManager = new MultiDatabaseManager(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), mockStoreWrapper.Object);
        multiDatabaseManager.Logger = mockLogger.Object;

        var mockDatabase = new Mock<GarnetDatabase>();
        mockDatabaseManager.Setup(dm => dm.TryGetOrAddDatabase(It.IsAny<int>(), out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny)).Returns(mockDatabase.Object);

        mockDatabaseManager.Setup(dm => dm.RecoverDatabaseCheckpoint(It.IsAny<GarnetDatabase>(), out It.Ref<long>.IsAny, out It.Ref<long>.IsAny)).Throws(new Exception("Test exception"));

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
