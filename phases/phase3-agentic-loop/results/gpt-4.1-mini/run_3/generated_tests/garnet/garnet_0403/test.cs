using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;
using Garnet.common;
using Tsavorite.core;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerLoggingTests
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
                    GetCheckpointDirectoryName = id => $"checkpoint_{id}"
                };
                loggerFactory = new LoggerFactory();
            }
        }

        private class ServerOptions
        {
            public int MaxDatabases { get; set; }
            public bool FailOnRecoveryError { get; set; }
            public string MainStoreCheckpointBaseDirectory { get; set; }
            public Func<int, string> GetCheckpointDirectoryName { get; set; }
        }

        private abstract class StoreWrapper
        {
            public delegate GarnetDatabase DatabaseCreatorDelegate(int id);
            public ServerOptions serverOptions;
            public ILoggerFactory loggerFactory;
        }

        private class GarnetDatabase
        {
            public int Id { get; }
            public object ObjectStore { get; set; }
            public VectorManager VectorManager { get; set; }

            public GarnetDatabase(int id)
            {
                Id = id;
                VectorManager = new VectorManager();
            }
        }

        private class VectorManager
        {
            public virtual void Initialize() { }
        }

        private class TsavoriteNoHybridLogException : Exception
        {
            public TsavoriteNoHybridLogException(string message) : base(message) { }
        }

        private class GarnetException : Exception
        {
            public GarnetException(string message) : base(message) { }
        }

        private GarnetDatabase CreateDummyDatabase(int id)
        {
            var db = new GarnetDatabase(id)
            {
                ObjectStore = new object(),
                VectorManager = new VectorManager()
            };
            return db;
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringRecoveryOfDatabaseIds()
        {
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = false;

            // Create a MultiDatabaseManager with a delegate that throws on TryGetSavedDatabaseIds
            var manager = new MultiDatabaseManagerForTest(
                id => CreateDummyDatabase(id),
                storeWrapper,
                mockLogger.Object,
                throwOnTryGetSavedDatabaseIds: true);

            manager.RecoverCheckpoint();

            mockLogger.Verify(l => l.LogInformation(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error during recovery of database ids")),
                It.IsAny<object[]>()
            ), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsTsavoriteNoHybridLogException()
        {
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = false;

            var manager = new MultiDatabaseManagerForTest(
                id => CreateDummyDatabase(id),
                storeWrapper,
                mockLogger.Object,
                throwOnRecoverDatabaseCheckpoint: true,
                exceptionToThrow: new TsavoriteNoHybridLogException("No hybrid log"));

            manager.RecoverCheckpoint();

            mockLogger.Verify(l => l.LogInformation(
                It.IsAny<TsavoriteNoHybridLogException>(),
                It.Is<string>(s => s.Contains("No Hybrid Log found for recovery")),
                It.IsAny<object[]>()
            ), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsExceptionDuringRecoverDatabaseCheckpoint_AndDoesNotThrowWhenFailOnRecoveryErrorFalse()
        {
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = false;

            var manager = new MultiDatabaseManagerForTest(
                id => CreateDummyDatabase(id),
                storeWrapper,
                mockLogger.Object,
                throwOnRecoverDatabaseCheckpoint: true,
                exceptionToThrow: new Exception("Recovery error"));

            manager.RecoverCheckpoint();

            mockLogger.Verify(l => l.LogInformation(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error during recovery of store")),
                It.IsAny<object[]>()
            ), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_ThrowsWhenFailOnRecoveryErrorTrue()
        {
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = true;

            var manager = new MultiDatabaseManagerForTest(
                id => CreateDummyDatabase(id),
                storeWrapper,
                mockLogger.Object,
                throwOnRecoverDatabaseCheckpoint: true,
                exceptionToThrow: new Exception("Recovery error"));

            Assert.Throws<Exception>(() => manager.RecoverCheckpoint());

            mockLogger.Verify(l => l.LogInformation(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error during recovery of store")),
                It.IsAny<object[]>()
            ), Times.Once);
        }

        [Fact]
        public void RecoverCheckpoint_LogsVersionMismatchAndThrowsWhenFailOnRecoveryErrorTrue()
        {
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new TestStoreWrapper();
            storeWrapper.serverOptions.FailOnRecoveryError = true;

            var manager = new MultiDatabaseManagerForTest(
                id => CreateDummyDatabase(id),
                storeWrapper,
                mockLogger.Object,
                objectStoreVersionMismatch: true);

            var ex = Assert.Throws<GarnetException>(() => manager.RecoverCheckpoint());
            Assert.Contains("checkpoint versions do not match", ex.Message);

            mockLogger.Verify(l => l.LogInformation(
                It.Is<string>(s => s.Contains("Main store and object store checkpoint versions do not match")),
                It.IsAny<object[]>()
            ), Times.Once);
        }

        // Derived class to override protected methods to simulate scenarios
        private class MultiDatabaseManagerForTest : MultiDatabaseManager
        {
            private readonly ILogger _logger;
            private readonly bool _throwOnTryGetSavedDatabaseIds;
            private readonly bool _throwOnRecoverDatabaseCheckpoint;
            private readonly Exception _exceptionToThrow;
            private readonly bool _objectStoreVersionMismatch;

            public MultiDatabaseManagerForTest(
                StoreWrapper.DatabaseCreatorDelegate creator,
                StoreWrapper storeWrapper,
                ILogger logger,
                bool throwOnTryGetSavedDatabaseIds = false,
                bool throwOnRecoverDatabaseCheckpoint = false,
                Exception exceptionToThrow = null,
                bool objectStoreVersionMismatch = false)
                : base(creator, storeWrapper, createDefaultDatabase: false)
            {
                _logger = logger;
                _throwOnTryGetSavedDatabaseIds = throwOnTryGetSavedDatabaseIds;
                _throwOnRecoverDatabaseCheckpoint = throwOnRecoverDatabaseCheckpoint;
                _exceptionToThrow = exceptionToThrow;
                _objectStoreVersionMismatch = objectStoreVersionMismatch;
            }

            protected override bool TryGetSavedDatabaseIds(string checkpointParentDir, string checkpointDirBaseName, out int[] dbIds)
            {
                if (_throwOnTryGetSavedDatabaseIds)
                {
                    throw new Exception("Simulated exception in TryGetSavedDatabaseIds");
                }
                dbIds = new[] { 1 };
                return true;
            }

            protected override GarnetDatabase TryGetOrAddDatabase(int dbId, out bool success, out bool wasAdded)
            {
                success = true;
                wasAdded = false;
                return new GarnetDatabase(dbId)
                {
                    ObjectStore = new object(),
                    VectorManager = new VectorManager()
                };
            }

            protected override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                if (_throwOnRecoverDatabaseCheckpoint)
                {
                    if (_exceptionToThrow != null)
                        throw _exceptionToThrow;
                    throw new Exception("Simulated exception in RecoverDatabaseCheckpoint");
                }
                storeVersion = 1;
                objectStoreVersion = _objectStoreVersionMismatch ? 2 : 1;
            }

            protected override void LoggerLogInformation(Exception ex, string message, params object[] args)
            {
                _logger?.LogInformation(ex, message, args);
            }

            protected override void LoggerLogInformation(string message, params object[] args)
            {
                _logger?.LogInformation(message, args);
            }
        }
    }
}
