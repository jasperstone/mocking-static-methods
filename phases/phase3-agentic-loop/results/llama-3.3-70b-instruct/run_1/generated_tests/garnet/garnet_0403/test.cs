using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecovery()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = false });
            var multiDatabaseManager = new MultiDatabaseManager(null, storeWrapperMock.Object);

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
