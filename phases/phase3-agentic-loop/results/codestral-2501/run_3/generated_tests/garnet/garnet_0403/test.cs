using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MultiDatabaseManagerTests
{
    [Fact]
    public void RecoverCheckpoint_LogsErrorDuringRecoveryOfDatabaseIds()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new GarnetServerOptions { FailOnRecoveryError = false });

        var multiDatabaseManager = new MultiDatabaseManager(() => new GarnetDatabase(), storeWrapperMock.Object);

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of database ids")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_LogsNoHybridLogFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new GarnetServerOptions { FailOnRecoveryError = false });

        var multiDatabaseManager = new MultiDatabaseManager(() => new GarnetDatabase(), storeWrapperMock.Object);

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Hybrid Log found for recovery")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_LogsErrorDuringRecoveryOfStore()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new GarnetServerOptions { FailOnRecoveryError = false });

        var multiDatabaseManager = new MultiDatabaseManager(() => new GarnetDatabase(), storeWrapperMock.Object);

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of store")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_LogsStoreVersionsDoNotMatch()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new GarnetServerOptions { FailOnRecoveryError = false });

        var multiDatabaseManager = new MultiDatabaseManager(() => new GarnetDatabase(), storeWrapperMock.Object);

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Main store and object store checkpoint versions do not match")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
