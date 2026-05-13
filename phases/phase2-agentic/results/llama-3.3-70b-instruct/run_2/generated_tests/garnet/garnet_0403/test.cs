using Xunit;
using Moq;
using System;
using System.Threading;
using Garnet.server;
using Microsoft.Extensions.Logging;

namespace GarnetTests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(new GarnetServerOptions());
            storeWrapperMock.SetupGet(sw => sw.loggerFactory).Returns(new LoggerFactory());
            var multiDatabaseManager = new MultiDatabaseManager(null, storeWrapperMock.Object);

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenTsavoriteNoHybridLogExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(new GarnetServerOptions());
            storeWrapperMock.SetupGet(sw => sw.loggerFactory).Returns(new LoggerFactory());
            var multiDatabaseManager = new MultiDatabaseManager(null, storeWrapperMock.Object);

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
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(new GarnetServerOptions());
            storeWrapperMock.SetupGet(sw => sw.loggerFactory).Returns(new LoggerFactory());
            var multiDatabaseManager = new MultiDatabaseManager(null, storeWrapperMock.Object);

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
