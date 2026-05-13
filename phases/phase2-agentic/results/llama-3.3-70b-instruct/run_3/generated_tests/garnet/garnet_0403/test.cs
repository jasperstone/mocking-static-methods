using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var multiDatabaseManager = new MultiDatabaseManager(null, null, false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenNoHybridLogFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var multiDatabaseManager = new MultiDatabaseManager(null, null, false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<TsavoriteNoHybridLogException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenStoreVersionsDoNotMatch()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var multiDatabaseManager = new MultiDatabaseManager(null, null, false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
