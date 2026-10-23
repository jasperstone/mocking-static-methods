using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.server
{
    internal class MultiDatabaseManagerTests
    {
        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            public TestMultiDatabaseManager(StoreWrapper storeWrapper) : base(
                (id) => throw new NotImplementedException(),
                storeWrapper,
                createDefaultDatabase: false)
            {
            }

            public Exception ExceptionToThrow { get; set; }

            protected override bool TryGetSavedDatabaseIds(string checkpointParentDir, string checkpointDirBaseName, out int[] dbIds)
            {
                if (ExceptionToThrow != null)
                    throw ExceptionToThrow;
                dbIds = null;
                return false;
            }
        }

        [Fact]
        public void RecoverCheckpoint_LogsInformationOnTryGetSavedDatabaseIdsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverOptions = new ServerOptions
            {
                FailOnRecoveryError = false,
                MainStoreCheckpointBaseDirectory = "baseDir",
                MaxDatabases = 1
            };
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var storeWrapper = new StoreWrapper(serverOptions, loggerFactoryMock.Object);

            var manager = new TestMultiDatabaseManager(storeWrapper);
            var testException = new InvalidOperationException("Test exception");
            manager.ExceptionToThrow = testException;

            // Act
            manager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    testException,
                    "Error during recovery of database ids; checkpointParentDir = {checkpointParentDir}; checkpointDirBaseName = {checkpointDirBaseName}",
                    serverOptions.MainStoreCheckpointBaseDirectory,
                    serverOptions.GetCheckpointDirectoryName(0)),
                Times.Once);
        }
    }
}
