using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecovery()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new StoreWrapper
            {
                loggerFactory = Mock.Of<ILoggerFactory>(),
                serverOptions = new ServerOptions { FailOnRecoveryError = false }
            };
            var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), storeWrapper);
            multiDatabaseManager.Logger = mockLogger.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of database ids")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringStoreRecovery()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new StoreWrapper
            {
                loggerFactory = Mock.Of<ILoggerFactory>(),
                serverOptions = new ServerOptions { FailOnRecoveryError = false }
            };
            var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), storeWrapper);
            multiDatabaseManager.Logger = mockLogger.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during recovery of store")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsNoHybridLogFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new StoreWrapper
            {
                loggerFactory = Mock.Of<ILoggerFactory>(),
                serverOptions = new ServerOptions { FailOnRecoveryError = false }
            };
            var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), storeWrapper);
            multiDatabaseManager.Logger = mockLogger.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Hybrid Log found for recovery")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsStoreVersionsDoNotMatch()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new StoreWrapper
            {
                loggerFactory = Mock.Of<ILoggerFactory>(),
                serverOptions = new ServerOptions { FailOnRecoveryError = false }
            };
            var multiDatabaseManager = new MultiDatabaseManager((dbId) => new GarnetDatabase(), storeWrapper);
            multiDatabaseManager.Logger = mockLogger.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Main store and object store checkpoint versions do not match")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
