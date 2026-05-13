using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;
using Garnet.common;
using Tsavorite.core;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        private class TestStoreWrapper : StoreWrapper
        {
            public TestStoreWrapper()
            {
                serverOptions = new ServerOptions
                {
                    MaxDatabases = 10,
                    FailOnRecoveryError = false,
                    MainStoreCheckpointBaseDirectory = "baseDir",
                    GetCheckpointDirectoryName = (id) => $"checkpoint_{id}"
                };
                loggerFactory = new LoggerFactory();
            }
        }

        private MultiDatabaseManager CreateManagerWithMocks(
            out Mock<ILogger> loggerMock,
            out List<(Exception ex, string message)> loggedMessages,
            bool failOnRecoveryError = false,
            int[] dbIdsToRecover = null,
            bool throwOnTryGetSavedDatabaseIds = false,
            bool throwOnRecoverDatabaseCheckpoint = false,
            bool throwTsavoriteNoHybridLogException = false,
            bool objectStoreVersionMismatch = false)
        {
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = failOnRecoveryError;

            var dbs = new Dictionary<int, GarnetDatabase>();

            var manager = new MultiDatabaseManager(id =>
            {
                var dbMock = new Mock<GarnetDatabase>();
                dbMock.SetupGet(d => d.ObjectStore).Returns(objectStoreVersionMismatch ? new object() : null);
                dbMock.SetupGet(d => d.VectorManager).Returns(new VectorManagerMock());
                dbs[id] = dbMock.Object;
                return dbMock.Object;
            }, storeWrapper, createDefaultDatabase: false);

            // Setup TryGetSavedDatabaseIds override via reflection or subclassing
            // Since it's private, we simulate by subclassing MultiDatabaseManager with override
            var managerMock = new Mock<MultiDatabaseManager>(storeWrapper.CreateDatabaseDelegate, storeWrapper, false)
            {
                CallBase = true
            };

            managerMock.Setup(m => m.TryGetSavedDatabaseIds(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<int[]>.IsAny))
                .Callback(new TryGetSavedDatabaseIdsDelegate((string parentDir, string baseName, out int[] ids) =>
                {
                    if (throwOnTryGetSavedDatabaseIds)
                    {
                        throw new Exception("Simulated exception in TryGetSavedDatabaseIds");
                    }
                    ids = dbIdsToRecover ?? new int[] { 1 };
                    return true;
                }))
                .Returns(true);

            managerMock.Setup(m => m.TryGetOrAddDatabase(It.IsAny<int>(), out It.Ref<bool>.IsAny, out It.Ref<GarnetDatabase>.IsAny))
                .Callback(new TryGetOrAddDatabaseDelegate((int id, out bool success, out GarnetDatabase db) =>
                {
                    if (dbs.TryGetValue(id, out var existingDb))
                    {
                        success = true;
                        db = existingDb;
                    }
                    else
                    {
                        success = true;
                        var dbMock = new Mock<GarnetDatabase>();
                        dbMock.SetupGet(d => d.ObjectStore).Returns(objectStoreVersionMismatch ? new object() : null);
                        dbMock.SetupGet(d => d.VectorManager).Returns(new VectorManagerMock());
                        db = dbMock.Object;
                        dbs[id] = db;
                    }
                }))
                .Returns((int id, out bool success, out GarnetDatabase db) => true);

            managerMock.Setup(m => m.RecoverDatabaseCheckpoint(It.IsAny<GarnetDatabase>(), out It.Ref<long>.IsAny, out It.Ref<long>.IsAny))
                .Callback(new RecoverDatabaseCheckpointDelegate((GarnetDatabase db, out long storeVersion, out long objectStoreVersion) =>
                {
                    if (throwTsavoriteNoHybridLogException)
                    {
                        throw new TsavoriteNoHybridLogException("No hybrid log");
                    }
                    if (throwOnRecoverDatabaseCheckpoint)
                    {
                        throw new Exception("Simulated exception in RecoverDatabaseCheckpoint");
                    }
                    storeVersion = 1;
                    objectStoreVersion = objectStoreVersionMismatch ? 2 : 1;
                }));

            loggedMessages = new List<(Exception ex, string message)>();
            loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback((LogLevel level, EventId eventId, object state, Exception ex, Func<object, Exception, string> formatter) =>
                {
                    var message = formatter(state, ex);
                    loggedMessages.Add((ex, message));
                });

            // Inject logger into manager
            managerMock.Object.Logger = loggerMock.Object;

            return managerMock.Object;
        }

        private delegate bool TryGetSavedDatabaseIdsDelegate(string checkpointParentDir, string checkpointDirBaseName, out int[] dbIds);
        private delegate bool TryGetOrAddDatabaseDelegate(int dbId, out bool success, out GarnetDatabase db);
        private delegate void RecoverDatabaseCheckpointDelegate(GarnetDatabase db, out long storeVersion, out long objectStoreVersion);

        private class VectorManagerMock : VectorManager
        {
            public bool Initialized { get; private set; }
            public override void Initialize()
            {
                Initialized = true;
            }
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecoveryOfDatabaseIds_AndDoesNotThrowWhenFailOnRecoveryErrorFalse()
        {
            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: false,
                throwOnTryGetSavedDatabaseIds: true);

            // Act
            manager.RecoverCheckpoint();

            // Assert
            Assert.Single(loggedMessages);
            var log = loggedMessages[0];
            Assert.NotNull(log.ex);
            Assert.Contains("Error during recovery of database ids", log.message);
        }

        [Fact]
        public void RecoverCheckpoint_ThrowsWhenFailOnRecoveryErrorTrueAndErrorDuringRecoveryOfDatabaseIds()
        {
            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: true,
                throwOnTryGetSavedDatabaseIds: true);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => manager.RecoverCheckpoint());
            Assert.Equal("Simulated exception in TryGetSavedDatabaseIds", ex.Message);
            Assert.Single(loggedMessages);
            Assert.Contains("Error during recovery of database ids", loggedMessages[0].message);
        }

        [Fact]
        public void RecoverCheckpoint_LogsTsavoriteNoHybridLogException()
        {
            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                throwTsavoriteNoHybridLogException: true);

            manager.RecoverCheckpoint();

            Assert.Single(loggedMessages);
            var log = loggedMessages[0];
            Assert.IsType<TsavoriteNoHybridLogException>(log.ex);
            Assert.Contains("No Hybrid Log found for recovery", log.message);
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecoveryOfStore_AndDoesNotThrowWhenFailOnRecoveryErrorFalse()
        {
            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: false,
                throwOnRecoverDatabaseCheckpoint: true);

            manager.RecoverCheckpoint();

            Assert.Single(loggedMessages);
            var log = loggedMessages[0];
            Assert.NotNull(log.ex);
            Assert.Contains("Error during recovery of store", log.message);
        }

        [Fact]
        public void RecoverCheckpoint_ThrowsWhenFailOnRecoveryErrorTrueAndErrorDuringRecoveryOfStore()
        {
            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: true,
                throwOnRecoverDatabaseCheckpoint: true);

            var ex = Record.Exception(() => manager.RecoverCheckpoint());

            Assert.NotNull(ex);
            Assert.Contains("Simulated exception in RecoverDatabaseCheckpoint", ex.Message);
            Assert.Single(loggedMessages);
            Assert.Contains("Error during recovery of store", loggedMessages[0].message);
        }

        [Fact]
        public void RecoverCheckpoint_LogsVersionMismatchAndThrowsWhenFailOnRecoveryErrorTrue()
        {
            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: true,
                objectStoreVersionMismatch: true);

            var ex = Assert.Throws<GarnetException>(() => manager.RecoverCheckpoint());

            Assert.Contains("Main store and object store checkpoint versions do not match", ex.Message);
            Assert.Contains("Main store and object store checkpoint versions do not match", loggedMessages[0].message);
        }

        [Fact]
        public void RecoverCheckpoint_InitializesVectorManager()
        {
            var initialized = false;
            var storeWrapper = new TestStoreWrapper();

            var manager = new MultiDatabaseManager(id =>
            {
                var dbMock = new Mock<GarnetDatabase>();
                dbMock.SetupGet(d => d.ObjectStore).Returns(null);
                var vectorManagerMock = new Mock<VectorManager>();
                vectorManagerMock.Setup(vm => vm.Initialize()).Callback(() => initialized = true);
                dbMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);
                return dbMock.Object;
            }, storeWrapper, createDefaultDatabase: false);

            // We need to override TryGetSavedDatabaseIds and TryGetOrAddDatabase to simulate recovery
            var managerMock = new Mock<MultiDatabaseManager>(storeWrapper.CreateDatabaseDelegate, storeWrapper, false)
            {
                CallBase = true
            };

            int[] dbIds = new int[] { 1 };
            managerMock.Setup(m => m.TryGetSavedDatabaseIds(It.IsAny<string>(), It.IsAny<string>(), out dbIds)).Returns(true);
            managerMock.Setup(m => m.TryGetOrAddDatabase(It.IsAny<int>(), out It.Ref<bool>.IsAny, out It.Ref<GarnetDatabase>.IsAny))
                .Callback((int id, out bool success, out GarnetDatabase db) =>
                {
                    success = true;
                    var dbMock = new Mock<GarnetDatabase>();
                    dbMock.SetupGet(d => d.ObjectStore).Returns(null);
                    var vectorManagerMock = new Mock<VectorManager>();
                    vectorManagerMock.Setup(vm => vm.Initialize()).Callback(() => initialized = true);
                    dbMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);
                    db = dbMock.Object;
                })
                .Returns(true);

            managerMock.Setup(m => m.RecoverDatabaseCheckpoint(It.IsAny<GarnetDatabase>(), out It.Ref<long>.IsAny, out It.Ref<long>.IsAny))
                .Callback((GarnetDatabase db, out long storeVersion, out long objectStoreVersion) =>
                {
                    storeVersion = 1;
                    objectStoreVersion = 1;
                });

            managerMock.Object.Logger = new Mock<ILogger>().Object;

            managerMock.Object.RecoverCheckpoint();

            Assert.True(initialized);
        }
    }
}
