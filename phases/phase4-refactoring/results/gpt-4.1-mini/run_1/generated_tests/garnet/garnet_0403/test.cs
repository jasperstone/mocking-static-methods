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
        public void RecoverCheckpoint_LogsInformationOnRecoverDatabaseCheckpointException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var options = new StoreWrapper.ServerOptions
            {
                MainStoreCheckpointBaseDirectory = "baseDir",
                FailOnRecoveryError = false,
                MaxDatabases = 10
            };
            var loggerFactory = new LoggerFactory();
            var storeWrapperMock = new Mock<StoreWrapper>((StoreWrapper.DatabaseCreatorDelegate)null, null, false);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(options);
            storeWrapperMock.SetupGet(s => s.loggerFactory).Returns(loggerFactory);

            // Create a dummy database that has ObjectStore not null and VectorManager with Initialize method
            var dummyDb = new DummyGarnetDatabase();

            // Create a MultiDatabaseManager with a delegate that returns dummyDb for any id
            var manager = new TestMultiDatabaseManager(id => dummyDb, storeWrapperMock.Object, loggerMock.Object);

            // Act
            manager.RecoverCheckpoint();

            // Assert
            // Verify that LogInformation was called with the expected message containing "Error during recovery of store"
            loggerMock.Verify(l => l.LogInformation(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error during recovery of store")),
                It.IsAny<object[]>()
            ), Times.Once);
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            private readonly ILogger _logger;

            public TestMultiDatabaseManager(StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate, StoreWrapper storeWrapper, ILogger logger)
                : base(createDatabaseDelegate, storeWrapper, false)
            {
                _logger = logger;
                var loggerFieldInfo = typeof(MultiDatabaseManager).GetField("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (loggerFieldInfo != null)
                    loggerFieldInfo.SetValue(this, _logger);
            }

            protected override bool TryGetSavedDatabaseIds(string checkpointParentDir, string checkpointDirBaseName, out int[] dbIds)
            {
                dbIds = new[] { 0 };
                return true;
            }

            protected override GarnetDatabase TryGetOrAddDatabase(int dbId, out bool success, out bool added)
            {
                success = true;
                added = false;
                return CreateDatabaseDelegate(dbId);
            }

            protected override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                storeVersion = 1;
                objectStoreVersion = 1;
                throw new Exception("Simulated exception in RecoverDatabaseCheckpoint");
            }
        }

        private class DummyGarnetDatabase : GarnetDatabase
        {
            public DummyGarnetDatabase() : base(null, null, null, null, null, null, null, null, null, null, null)
            {
                ObjectStore = new DummyObjectStore();
                VectorManager = new DummyVectorManager();
            }

            public override IGarnetObjectStore ObjectStore { get; }

            public override IVectorManager VectorManager { get; }

            private class DummyObjectStore : IGarnetObjectStore
            {
                public long GetRecoverVersion() => 1;
                public long Recover(long recoverTo) => 1;
            }

            private class DummyVectorManager : IVectorManager
            {
                public void Initialize() { }
            }
        }
    }
}
