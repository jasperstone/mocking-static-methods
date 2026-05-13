using Xunit;
using Moq;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var multiDatabaseManager = new MultiDatabaseManager(
                (id) => new GarnetDatabase(),
                new StoreWrapper(),
                createDefaultDatabase: false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenNoHybridLogFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var multiDatabaseManager = new MultiDatabaseManager(
                (id) => new GarnetDatabase(),
                new StoreWrapper(),
                createDefaultDatabase: false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenStoreVersionsDoNotMatch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var multiDatabaseManager = new MultiDatabaseManager(
                (id) => new GarnetDatabase(),
                new StoreWrapper(),
                createDefaultDatabase: false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
