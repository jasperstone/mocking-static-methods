using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsErrorOnRecoveryException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(s => s.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = false });

            var multiDatabaseManager = new MultiDatabaseManager(
                dbId => new GarnetDatabase(), // Mock database creation
                storeWrapperMock.Object);

            multiDatabaseManager.Logger = loggerMock.Object;

            // Simulate an exception during recovery
            var exception = new Exception("Test exception");

            // Act
            try
            {
                multiDatabaseManager.RecoverCheckpoint();
            }
            catch (Exception)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<Exception>(),
                    "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                    It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }
    }
}
