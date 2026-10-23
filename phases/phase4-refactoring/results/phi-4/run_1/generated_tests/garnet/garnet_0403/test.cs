using System;
using System.Threading;
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
            var createDatabaseDelegate = new StoreWrapper.DatabaseCreatorDelegate((id) => new GarnetDatabase());

            var manager = new MultiDatabaseManager(createDatabaseDelegate, storeWrapperMock.Object);
            manager.Logger = loggerMock.Object;

            // Simulate failure in TryGetSavedDatabaseIds
            manager.TryGetSavedDatabaseIds = (parentDir, dirBaseName, out int[] dbIds) =>
            {
                dbIds = null;
                return false;
            };

            // Act
            manager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<Exception>(),
                    "Error during recovery of database ids; checkpointParentDir = {checkpointParentDir}; checkpointDirBaseName = {checkpointDirBaseName}",
                    It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
