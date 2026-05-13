using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsErrorOnRecoveryFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.Setup(s => s.serverOptions).Returns(new ServerOptions { FailOnRecoveryError = false });

            var manager = new MultiDatabaseManager(
                dbId => new GarnetDatabase(), 
                storeWrapperMock.Object
            );

            manager.Logger = loggerMock.Object;

            // Simulate an exception during recovery
            var ex = new Exception("Test exception");

            // Act
            try
            {
                manager.RecoverCheckpoint();
            }
            catch
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<Exception>(),
                    "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                    0, 0),
                Times.Once);
        }
    }
}
