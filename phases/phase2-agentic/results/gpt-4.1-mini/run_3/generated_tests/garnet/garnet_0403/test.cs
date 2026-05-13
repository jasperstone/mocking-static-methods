using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;
using Garnet.common;

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
            bool throwOnTryGetOrAdd = false,
            bool throwOnRecoverDatabaseCheckpoint = false,
            Exception recoverException = null,
            bool objectStoreVersionMismatch = false)
        {
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = failOnRecoveryError;

            // Create a MultiDatabaseManager with a delegate that returns a mocked GarnetDatabase
            var dbs = new Dictionary<int, GarnetDatabase>();
            var manager = new MultiDatabaseManager(id =>
            {
                var dbMock = new Mock<GarnetDatabase>();
                dbMock.SetupGet(d => d.ObjectStore).Returns(objectStoreVersionMismatch ? new object() : null);
                dbMock.SetupGet(d => d.VectorManager).Returns(new VectorManagerMock());
                dbs[id] = dbMock.Object;
                return dbMock.Object;
            }, storeWrapper, createDefaultDatabase: false);

            // Setup TryGetSavedDatabaseIds to return the provided dbIdsToRecover
            var tryGetSavedDatabaseIdsMethod = typeof(MultiDatabaseManager).GetMethod("TryGetSavedDatabaseIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // We cannot override private method easily, so we will simulate by reflection or by subclassing
            // Instead, we will subclass MultiDatabaseManager to override RecoverCheckpoint for test

            // Setup TryGetOrAddDatabase to return db from dictionary or throw
            var tryGetOrAddDatabaseMethod = typeof(MultiDatabaseManager).GetMethod("TryGetOrAddDatabase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // We will subclass MultiDatabaseManager to override TryGetSavedDatabaseIds and TryGetOrAddDatabase and RecoverDatabaseCheckpoint

            loggedMessages = new List<(Exception, string)>();
            loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback((LogLevel level, EventId eventId, object state, Exception ex, Delegate formatter) =>
                {
                    var message = formatter.DynamicInvoke(state, ex) as string;
                    loggedMessages.Add((ex, message));
                });

            var testManager = new TestMultiDatabaseManager(
                id =>
                {
                    var dbMock = new Mock<GarnetDatabase>();
                    dbMock.SetupGet(d => d.ObjectStore).Returns(objectStoreVersionMismatch ? new object() : null);
                    dbMock.SetupGet(d => d.VectorManager).Returns(new VectorManagerMock());
                    dbs[id] = dbMock.Object;
                    return dbMock.Object;
                },
                storeWrapper,
                dbIdsToRecover,
                throwOnTryGetOrAdd,
                throwOnRecoverDatabaseCheckpoint,
                recoverException,
                failOnRecoveryError,
                loggerMock.Object);

            return testManager;
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            private readonly int[] _dbIdsToRecover;
            private readonly bool _throwOnTryGetOrAdd;
            private readonly bool _throwOnRecoverDatabaseCheckpoint;
            private readonly Exception _recoverException;
            private readonly bool _failOnRecoveryError;
            private readonly ILogger _testLogger;

            public TestMultiDatabaseManager(
                StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate,
                StoreWrapper storeWrapper,
                int[] dbIdsToRecover,
                bool throwOnTryGetOrAdd,
                bool throwOnRecoverDatabaseCheckpoint,
                Exception recoverException,
                bool failOnRecoveryError,
                ILogger testLogger) : base(createDatabaseDelegate, storeWrapper, false)
            {
                _dbIdsToRecover = dbIdsToRecover;
                _throwOnTryGetOrAdd = throwOnTryGetOrAdd;
                _throwOnRecoverDatabaseCheckpoint = throwOnRecoverDatabaseCheckpoint;
                _recoverException = recoverException;
                _failOnRecoveryError = failOnRecoveryError;
                _testLogger = testLogger;
                Logger = testLogger;
                StoreWrapper.serverOptions.FailOnRecoveryError = failOnRecoveryError;
            }

            protected override bool TryGetSavedDatabaseIds(string checkpointParentDir, string checkpointDirBaseName, out int[] dbIds)
            {
                if (_dbIdsToRecover == null)
                {
                    dbIds = null;
                    return false;
                }
                dbIds = _dbIdsToRecover;
                return true;
            }

            protected override GarnetDatabase TryGetOrAddDatabase(int dbId, out bool success, out bool created)
            {
                if (_throwOnTryGetOrAdd)
                {
                    success = false;
                    created = false;
                    return null;
                }
                success = true;
                created = true;
                return base.CreateDatabaseDelegate(dbId);
            }

            protected override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                if (_throwOnRecoverDatabaseCheckpoint)
                {
                    storeVersion = 0;
                    objectStoreVersion = 0;
                    if (_recoverException != null)
                        throw _recoverException;
                    throw new Exception("Test exception");
                }
                storeVersion = 1;
                objectStoreVersion = 1;
            }
        }

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
            // Arrange
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = false;

            var loggerMock = new Mock<ILogger>();
            var loggedMessages = new List<(Exception ex, string message)>();
            loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback((LogLevel level, EventId eventId, object state, Exception ex, Delegate formatter) =>
                {
                    var message = formatter.DynamicInvoke(state, ex) as string;
                    loggedMessages.Add((ex, message));
                });

            var manager = new MultiDatabaseManager(id => new GarnetDatabaseMock(), storeWrapper, false);
            manager.Logger = loggerMock.Object;

            // We simulate TryGetSavedDatabaseIds throwing by subclassing and overriding RecoverCheckpoint
            var exToThrow = new Exception("Simulated exception");
            var testManager = new TestMultiDatabaseManager(
                id => new GarnetDatabaseMock(),
                storeWrapper,
                null,
                false,
                false,
                null,
                false,
                loggerMock.Object);

            // Act
            // We override TryGetSavedDatabaseIds to throw
            var exThrown = false;
            try
            {
                testManager.RecoverCheckpoint();
            }
            catch
            {
                exThrown = true;
            }

            // Assert
            Assert.False(exThrown);
            Assert.Single(loggedMessages);
            Assert.Equal("Error during recovery of database ids; checkpointParentDir = baseDir; checkpointDirBaseName = checkpoint_0", loggedMessages[0].message);
            Assert.IsType<Exception>(loggedMessages[0].ex);
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecoveryOfStore_AndDoesNotThrowWhenFailOnRecoveryErrorFalse()
        {
            // Arrange
            var dbIds = new[] { 1 };
            var exToThrow = new Exception("Recovery error");

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: false,
                dbIdsToRecover: dbIds,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: true,
                recoverException: exToThrow,
                objectStoreVersionMismatch: false);

            // Act
            var exThrown = false;
            try
            {
                manager.RecoverCheckpoint();
            }
            catch
            {
                exThrown = true;
            }

            // Assert
            Assert.False(exThrown);
            Assert.Single(loggedMessages);
            Assert.Contains("Error during recovery of store; storeVersion = 0; objectStoreVersion = 0", loggedMessages[0].message);
            Assert.Equal(exToThrow, loggedMessages[0].ex);
        }

        [Fact]
        public void RecoverCheckpoint_ThrowsWhenFailOnRecoveryErrorTrueAndRecoveryExceptionThrown()
        {
            // Arrange
            var dbIds = new[] { 1 };
            var exToThrow = new Exception("Recovery error");

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: true,
                dbIdsToRecover: dbIds,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: true,
                recoverException: exToThrow,
                objectStoreVersionMismatch: false);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => manager.RecoverCheckpoint());
            Assert.Equal(exToThrow, ex);
            Assert.Single(loggedMessages);
            Assert.Contains("Error during recovery of store; storeVersion = 0; objectStoreVersion = 0", loggedMessages[0].message);
            Assert.Equal(exToThrow, loggedMessages[0].ex);
        }

        [Fact]
        public void RecoverCheckpoint_LogsVersionMismatchAndThrowsWhenFailOnRecoveryErrorTrue()
        {
            // Arrange
            var dbIds = new[] { 1 };

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: true,
                dbIdsToRecover: dbIds,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: false,
                recoverException: null,
                objectStoreVersionMismatch: true);

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() => manager.RecoverCheckpoint());
            Assert.Equal("Main store and object store checkpoint versions do not match", ex.Message);
            Assert.Single(loggedMessages);
            Assert.Contains("Main store and object store checkpoint versions do not match; storeVersion = 1; objectStoreVersion = 1", loggedMessages[0].message);
            Assert.Null(loggedMessages[0].ex);
        }

        [Fact]
        public void RecoverCheckpoint_LogsNoHybridLogFoundException()
        {
            // Arrange
            var dbIds = new[] { 1 };
            var exToThrow = new TsavoriteNoHybridLogException("No hybrid log");

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError: false,
                dbIdsToRecover: dbIds,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: true,
                recoverException: exToThrow,
                objectStoreVersionMismatch: false);

            // We override RecoverDatabaseCheckpoint to throw TsavoriteNoHybridLogException only once
            var testManager = manager as TestMultiDatabaseManager;
            testManager._throwOnRecoverDatabaseCheckpoint = false;
            testManager.RecoverDatabaseCheckpoint = (db, out long sv, out long osv) =>
            {
                sv = 0;
                osv = 0;
                throw exToThrow;
            };

            // Act
            var exThrown = false;
            try
            {
                testManager.RecoverCheckpoint();
            }
            catch
            {
                exThrown = true;
            }

            // Assert
            Assert.False(exThrown);
            Assert.Single(loggedMessages);
            Assert.Contains("No Hybrid Log found for recovery; storeVersion = 0; objectStoreVersion = 0", loggedMessages[0].message);
            Assert.Equal(exToThrow, loggedMessages[0].ex);
        }

        private class GarnetDatabaseMock : GarnetDatabase
        {
            public override object ObjectStore => null;
            public override VectorManager VectorManager { get; } = new VectorManagerMock();
        }
    }
}
