using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;
using Garnet.server;
using System.Reflection;
using System.IO;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerLoggerTests
    {
        private readonly Mock<ILogger<MultiDatabaseManager>> _loggerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<StoreWrapper.DatabaseCreatorDelegate> _createDatabaseDelegateMock;
        private readonly Mock<GarnetServerOptions> _serverOptionsMock;

        public MultiDatabaseManagerLoggerTests()
        {
            _loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _serverOptionsMock = new Mock<GarnetServerOptions>();
            _serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
            _serverOptionsMock.Setup(o => o.MainStoreCheckpointBaseDirectory).Returns("/invalid/path");

            _storeWrapperMock = new Mock<StoreWrapper>();
            _storeWrapperMock.Setup(s => s.serverOptions).Returns(_serverOptionsMock.Object);
            _storeWrapperMock.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);

            _createDatabaseDelegateMock = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
        }

        [Fact]
        public void RecoverCheckpoint_TryGetSavedDatabaseIdsThrows_LogsErrorDuringDatabaseIdsRecovery()
        {
            // Arrange
            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);

            // Act - invalid paths cause TryGetSavedDatabaseIds to throw DirectoryNotFoundException
            manager.RecoverCheckpoint();

            // Assert - verifies first LogInformation call
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<Exception>(),
                    "Error during recovery of database ids; checkpointParentDir = {checkpointParentDir}; checkpointDirBaseName = {checkpointDirBaseName}",
                    "/invalid/path",
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_RecoverDatabaseCheckpointThrowsGenericException_LogsErrorDuringStoreRecovery()
        {
            // Arrange - setup to pass database ID recovery, fail on RecoverDatabaseCheckpoint
            _createDatabaseDelegateMock.Setup(d => d(It.IsAny<int>())).Returns(CreateMockDatabase());
            
            // Temporarily override server options to pass ID recovery
            _serverOptionsMock.Setup(o => o.MainStoreCheckpointBaseDirectory).Returns(Directory.GetCurrentDirectory());
            
            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);

            // Act - will hit the generic exception catch block (line 137 target)
            // By creating database but making recovery fail naturally
            manager.RecoverCheckpoint();

            // Assert - verifies the specific LogInformation call on line 137
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<Exception>(),
                    "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                    0L,
                    0L),
                Times.AtLeastOnce);
        }

        [Fact]
        public void RecoverCheckpoint_AllowsTsavoriteNoHybridLogException_LogsAppropriately()
        {
            // Arrange
            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);
            
            // Act & Assert - this path logs the specific "No Hybrid Log" message
            // Natural execution hits this when no hybrid log exists
            manager.RecoverCheckpoint();
            
            // Verify the NoHybridLog specific log was called
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<Tsavorite.core.TsavoriteNoHybridLogException>(ex => ex != null),
                    "No Hybrid Log found for recovery; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                    0L,
                    0L),
                Times.AtLeastOnce);
        }

        [Fact]
        public void RecoverCheckpoint_LogsVersionMismatchWhenObjectStoreVersionsDiffer()
        {
            // Arrange
            var dbMock = new Mock<GarnetDatabase>();
            dbMock.Setup(d => d.ObjectStore).Returns(new Mock<Tsavorite.core.IObjectStore>().Object);
            dbMock.Setup(d => d.VectorManager).Returns(new Mock<GarnetVectorManager>().Object);
            _createDatabaseDelegateMock.Setup(d => d(It.IsAny<int>())).Returns(dbMock.Object);
            
            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);

            // Act
            manager.RecoverCheckpoint();

            // Assert - version mismatch log
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Main store and object store checkpoint versions do not match; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                    It.IsAny<long>(),
                    It.IsAny<long>()),
                Times.AtLeastOnce);
        }

        private static GarnetDatabase CreateMockDatabase()
        {
            var db = new Mock<GarnetDatabase>();
            db.Setup(d => d.VectorManager).Returns(new Mock<GarnetVectorManager>().Object);
            db.Setup(d => d.ObjectStore).Returns((Tsavorite.core.IObjectStore)null);
            return db.Object;
        }
    }
}
