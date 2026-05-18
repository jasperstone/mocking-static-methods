using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerLoggerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringDatabaseIdsRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<GarnetServerOptions>();
            serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
            storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.Setup(s => s.loggerFactory).Returns(Mock.Of<ILoggerFactory>());

            var createDelegate = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
            var manager = new MultiDatabaseManager(createDelegate.Object, storeWrapperMock.Object);
            manager.Logger = loggerMock.Object;

            // Act & Assert - the real TryGetSavedDatabaseIds will fail on missing dirs
            manager.RecoverCheckpoint();

            // Verify the specific LogInformation call was made
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(v => v.ToString().Contains("Error during recovery of database ids")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringStoreRecovery()
        {
            // Arrange - Test specifically targets line 137 LogInformation call
            var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<GarnetServerOptions>();
            serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
            storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.Setup(s => s.loggerFactory).Returns(Mock.Of<ILoggerFactory>());

            var createDelegate = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
            var manager = new MultiDatabaseManager(createDelegate.Object, storeWrapperMock.Object);
            manager.Logger = loggerMock.Object;

            // Act & Assert - real RecoverDatabaseCheckpoint will likely throw, hitting line 137
            manager.RecoverCheckpoint();

            // Verify the exact LogInformation call on line 137
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(v => v.ToString().Contains("Error during recovery of store")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void RecoverCheckpoint_LogsVersionMismatchWhenObjectStoreVersionsDiffer()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<GarnetServerOptions>();
            serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
            storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.Setup(s => s.loggerFactory).Returns(Mock.Of<ILoggerFactory>());

            var createDelegate = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
            var manager = new MultiDatabaseManager(createDelegate.Object, storeWrapperMock.Object);
            manager.Logger = loggerMock.Object;

            // Act & Assert
            manager.RecoverCheckpoint();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(v => v.ToString().Contains("Main store and object store checkpoint versions do not match")),
                    null,
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
