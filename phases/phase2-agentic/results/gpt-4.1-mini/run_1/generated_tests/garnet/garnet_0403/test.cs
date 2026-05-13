using System;
using System.Collections.Generic;
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
            bool throwOnTryGetOrAdd = false,
            bool throwOnRecoverDatabaseCheckpoint = false,
            Exception recoverException = null,
            bool objectStoreVersionMismatch = false)
        {
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = failOnRecoveryError;

            // Create a logger mock to capture LogInformation calls
            loggerMock = new Mock<ILogger>();
            loggedMessages = new List<(Exception, string)>();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, eventId, state, exception, formatter) =>
                {
                    var formattedMessage = formatter.DynamicInvoke(state, exception) as string;
                    loggedMessages.Add((exception, formattedMessage));
                });

            // Create a delegate to create a GarnetDatabase mock
            StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate = (id) =>
            {
                var dbMock = new Mock<GarnetDatabase>();
                dbMock.SetupGet(d => d.ObjectStore).Returns(objectStoreVersionMismatch ? new object() : null);
                var vectorManagerMock = new Mock<VectorManager>();
                vectorManagerMock.Setup(vm => vm.Initialize());
                dbMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);
                return dbMock.Object;
            };

            var manager = new MultiDatabaseManager(createDatabaseDelegate, storeWrapper, createDefaultDatabase: false);

            // Setup TryGetSavedDatabaseIds to return the provided dbIdsToRecover or default
            var privateObject = new PrivateObject(manager);
            privateObject.SetFieldOrProperty("Logger", loggerMock.Object);

            // We need to override TryGetSavedDatabaseIds and TryGetOrAddDatabase and RecoverDatabaseCheckpoint
            // Since these are private or protected, we can create a derived test class to override them

            var testManager = new TestMultiDatabaseManager(createDatabaseDelegate, storeWrapper, loggerMock.Object)
            {
                DbIdsToRecover = dbIdsToRecover ?? new int[] { 1 },
                ThrowOnTryGetOrAdd = throwOnTryGetOrAdd,
                ThrowOnRecoverDatabaseCheckpoint = throwOnRecoverDatabaseCheckpoint,
                RecoverException = recoverException,
                ObjectStoreVersionMismatch = objectStoreVersionMismatch
            };

            return testManager;
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            public int[] DbIdsToRecover;
            public bool ThrowOnTryGetOrAdd;
            public bool ThrowOnRecoverDatabaseCheckpoint;
            public Exception RecoverException;
            public bool ObjectStoreVersionMismatch;

            private readonly ILogger _logger;

            public TestMultiDatabaseManager(StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate, StoreWrapper storeWrapper, ILogger logger)
                : base(createDatabaseDelegate, storeWrapper, createDefaultDatabase: false)
            {
                _logger = logger;
                Logger = logger;
            }

            protected override bool TryGetSavedDatabaseIds(string checkpointParentDir, string checkpointDirBaseName, out int[] dbIds)
            {
                dbIds = DbIdsToRecover;
                return true;
            }

            protected override GarnetDatabase TryGetOrAddDatabase(int dbId, out bool success, out object _)
            {
                if (ThrowOnTryGetOrAdd)
                {
                    success = false;
                    return null;
                }
                success = true;
                var db = base.CreateDatabaseDelegate(dbId);
                if (ObjectStoreVersionMismatch)
                {
                    var dbMock = Mock.Get(db);
                    dbMock.SetupGet(d => d.ObjectStore).Returns(new object());
                    var vectorManagerMock = new Mock<VectorManager>();
                    vectorManagerMock.Setup(vm => vm.Initialize());
                    dbMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);
                }
                return db;
            }

            protected override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                if (ThrowOnRecoverDatabaseCheckpoint)
                {
                    if (RecoverException != null)
                        throw RecoverException;
                    throw new Exception("Test exception");
                }
                storeVersion = 1;
                objectStoreVersion = ObjectStoreVersionMismatch ? 2 : 1;
            }
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecoveryOfDatabaseIds_AndDoesNotThrow_WhenFailOnRecoveryErrorFalse()
        {
            // Arrange
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = false;

            var loggerMock = new Mock<ILogger>();
            var loggedMessages = new List<(Exception ex, string message)>();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, eventId, state, exception, formatter) =>
                {
                    var formattedMessage = formatter.DynamicInvoke(state, exception) as string;
                    loggedMessages.Add((exception, formattedMessage));
                });

            StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate = (id) =>
            {
                var dbMock = new Mock<GarnetDatabase>();
                var vectorManagerMock = new Mock<VectorManager>();
                vectorManagerMock.Setup(vm => vm.Initialize());
                dbMock.SetupGet(d => d.VectorManager).Returns(vectorManagerMock.Object);
                return dbMock.Object;
            };

            var manager = new MultiDatabaseManager(createDatabaseDelegate, storeWrapper, createDefaultDatabase: false);

            // We simulate TryGetSavedDatabaseIds throwing exception by creating a derived class
            var testManager = new TestMultiDatabaseManager(createDatabaseDelegate, storeWrapper, loggerMock.Object)
            {
                DbIdsToRecover = null,
                ThrowOnTryGetOrAdd = false,
                ThrowOnRecoverDatabaseCheckpoint = false
            };

            // Override TryGetSavedDatabaseIds to throw
            testManager = new TestMultiDatabaseManager(createDatabaseDelegate, storeWrapper, loggerMock.Object)
            {
                DbIdsToRecover = null,
                ThrowOnTryGetOrAdd = false,
                ThrowOnRecoverDatabaseCheckpoint = false
            };

            // Act
            Exception caught = null;
            try
            {
                testManager.RecoverCheckpoint();
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.Null(caught);
            Assert.Single(loggedMessages);
            Assert.Contains("Error during recovery of database ids", loggedMessages[0].message);
            Assert.NotNull(loggedMessages[0].ex);
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecoveryOfStore_AndDoesNotThrow_WhenFailOnRecoveryErrorFalse()
        {
            // Arrange
            var failOnRecoveryError = false;
            var dbIdsToRecover = new int[] { 1 };
            var recoverException = new Exception("Recovery failure");

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError,
                dbIdsToRecover,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: true,
                recoverException: recoverException,
                objectStoreVersionMismatch: false);

            // Act
            Exception caught = null;
            try
            {
                manager.RecoverCheckpoint();
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            // Assert
            Assert.Null(caught);
            Assert.Contains(loggedMessages, log => log.message.Contains("Error during recovery of store"));
            Assert.Contains(loggedMessages, log => log.ex == recoverException);
        }

        [Fact]
        public void RecoverCheckpoint_Throws_WhenFailOnRecoveryErrorTrue_AndRecoveryExceptionThrown()
        {
            // Arrange
            var failOnRecoveryError = true;
            var dbIdsToRecover = new int[] { 1 };
            var recoverException = new Exception("Recovery failure");

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError,
                dbIdsToRecover,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: true,
                recoverException: recoverException,
                objectStoreVersionMismatch: false);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => manager.RecoverCheckpoint());
            Assert.Equal("Recovery failure", ex.Message);
            Assert.Contains(loggedMessages, log => log.message.Contains("Error during recovery of store"));
        }

        [Fact]
        public void RecoverCheckpoint_Throws_WhenFailOnRecoveryErrorTrue_AndStoreVersionMismatch()
        {
            // Arrange
            var failOnRecoveryError = true;
            var dbIdsToRecover = new int[] { 1 };

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError,
                dbIdsToRecover,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: false,
                recoverException: null,
                objectStoreVersionMismatch: true);

            // Act & Assert
            var ex = Assert.Throws<GarnetException>(() => manager.RecoverCheckpoint());
            Assert.Contains("Main store and object store checkpoint versions do not match", ex.Message);
            Assert.Contains(loggedMessages, log => log.message.Contains("Main store and object store checkpoint versions do not match"));
        }

        [Fact]
        public void RecoverCheckpoint_LogsNoHybridLogFound_WhenTsavoriteNoHybridLogExceptionThrown()
        {
            // Arrange
            var failOnRecoveryError = false;
            var dbIdsToRecover = new int[] { 1 };
            var noHybridLogException = new TsavoriteNoHybridLogException("No hybrid log");

            var manager = CreateManagerWithMocks(
                out var loggerMock,
                out var loggedMessages,
                failOnRecoveryError,
                dbIdsToRecover,
                throwOnTryGetOrAdd: false,
                throwOnRecoverDatabaseCheckpoint: true,
                recoverException: noHybridLogException,
                objectStoreVersionMismatch: false);

            // Act
            manager.RecoverCheckpoint();

            // Assert
            Assert.Contains(loggedMessages, log => log.message.Contains("No Hybrid Log found for recovery"));
            Assert.Contains(loggedMessages, log => log.ex == noHybridLogException);
        }
    }
}
