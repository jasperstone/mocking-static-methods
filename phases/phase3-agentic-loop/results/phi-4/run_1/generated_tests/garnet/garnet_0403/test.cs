using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenRecoveryOfDatabaseIdsFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var multiDatabaseManager = new MultiDatabaseManager(
                dbId => new GarnetDatabase(), // Mock database creation
                storeWrapperMock.Object);

            multiDatabaseManager.Logger = loggerMock.Object;

            // Simulate failure in recovery
            storeWrapperMock.Setup(s => s.serverOptions.FailOnRecoveryError).Returns(false);

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
                x => x.LogInformation(
                    It.IsAny<Exception>(),
                    "Error during recovery of database ids; checkpointParentDir = {checkpointParentDir}; checkpointDirBaseName = {checkpointDirBaseName}",
                    It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
