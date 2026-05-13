using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.common;
using Garnet.server;
using Garnet.server.Metrics;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

public class MultiDatabaseManagerTests
{
    [Fact]
    public void RecoverCheckpoint_LogsErrorDuringRecoveryOfDatabaseIds()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServerOptions = new Mock<ServerOptions>();
        var mockDatabaseManagerBase = new Mock<DatabaseManagerBase>(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), mockStoreWrapper.Object);

        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());

        var multiDatabaseManager = new MultiDatabaseManager(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), mockStoreWrapper.Object);
        multiDatabaseManager.Logger = mockLogger.Object;

        var checkpointParentDir = "testParentDir";
        var checkpointDirBaseName = "testDirBaseName";

        mockDatabaseManagerBase.Setup(dmb => dmb.TryGetSavedDatabaseIds(checkpointParentDir, checkpointDirBaseName, out It.Ref<int[]>.IsAny))
            .Throws(new Exception("Test exception"));

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of database ids")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_LogsErrorDuringRecoveryOfStore()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServerOptions = new Mock<ServerOptions>();
        var mockDatabaseManagerBase = new Mock<DatabaseManagerBase>(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), mockStoreWrapper.Object);

        mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(mockServerOptions.Object);
        mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());

        var multiDatabaseManager = new MultiDatabaseManager(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), mockStoreWrapper.Object);
        multiDatabaseManager.Logger = mockLogger.Object;

        var checkpointParentDir = "testParentDir";
        var checkpointDirBaseName = "testDirBaseName";

        mockDatabaseManagerBase.Setup(dmb => dmb.TryGetSavedDatabaseIds(checkpointParentDir, checkpointDirBaseName, out It.Ref<int[]>.IsAny))
            .Returns(true);

        var dbIdsToRecover = new[] { 1 };
        mockDatabaseManagerBase.Setup(dmb => dmb.TryGetOrAddDatabase(It.IsAny<int>(), out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny))
            .Returns(Mock.Of<GarnetDatabase>());

        mockDatabaseManagerBase.Setup(dmb => dmb.RecoverDatabaseCheckpoint(It.IsAny<GarnetDatabase>(), out It.Ref<long>.IsAny, out It.Ref<long>.IsAny))
            .Throws(new Exception("Test exception"));

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of store")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
