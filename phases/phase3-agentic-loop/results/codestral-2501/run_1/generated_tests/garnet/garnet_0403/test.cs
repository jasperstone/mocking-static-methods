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
    public void RecoverCheckpoint_LogsErrorOnException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = true });

        var multiDatabaseManager = new MultiDatabaseManager(
            (dbId) => new GarnetDatabase(dbId, storeWrapperMock.Object),
            storeWrapperMock.Object);

        var exception = new Exception("Test exception");

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_LogsNoHybridLogFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = true });

        var multiDatabaseManager = new MultiDatabaseManager(
            (dbId) => new GarnetDatabase(dbId, storeWrapperMock.Object),
            storeWrapperMock.Object);

        var exception = new TsavoriteNoHybridLogException("Test exception");

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_LogsStoreVersionMismatch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = true });

        var multiDatabaseManager = new MultiDatabaseManager(
            (dbId) => new GarnetDatabase(dbId, storeWrapperMock.Object),
            storeWrapperMock.Object);

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
