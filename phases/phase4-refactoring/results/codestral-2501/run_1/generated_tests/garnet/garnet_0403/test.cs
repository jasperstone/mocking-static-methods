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
        public void RecoverCheckpoint_LogsInformationOnError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockDatabase = new Mock<GarnetDatabase>();

            mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(() => new Mock<ILoggerFactory>().Object);
            mockStoreWrapper.Setup(sw => sw.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = true });

            var multiDatabaseManager = new MultiDatabaseManager(
                dbId => mockDatabase.Object,
                mockStoreWrapper.Object,
                createDefaultDatabase: false
            );

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
