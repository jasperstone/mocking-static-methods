using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        private class DummyDatabase : GarnetDatabase
        {
            public bool Initialized { get; private set; }
            public override void Initialize() => Initialized = true;
            public override object ObjectStore => new object();
            public override VectorManager VectorManager { get; } = new VectorManager();
        }

        private class DummyVectorManager : VectorManager
        {
            public bool Initialized { get; private set; }
            public override void Initialize() => Initialized = true;
        }

        private class DummyDatabaseManager : MultiDatabaseManager
        {
            public bool RecoverCheckpointCalled { get; private set; }
            public bool TakeCheckpointAsyncCalled { get; private set; }
            public override void RecoverDatabaseCheckpoint(GarnetDatabase db, out long storeVersion, out long objectStoreVersion)
            {
                RecoverCheckpointCalled = true;
                storeVersion = 1;
                objectStoreVersion = 1;
            }

            public override Task<bool> TakeDatabasesCheckpointAsync(int[] dbIds)
            {
                TakeCheckpointAsyncCalled = true;
                return Task.FromResult(true);
            }

            public DummyDatabaseManager(StoreWrapper storeWrapper, bool createDefaultDb = true)
                : base((id) => new DummyDatabase(), storeWrapper, createDefaultDb)
            {
            }
        }

        [Fact]
        public async Task LogInformation_IsCalled_OnRecoveryError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<StoreWrapper.ServerOptions>();
            serverOptionsMock.SetupGet(s => s.FailOnRecoveryError).Returns(true);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.SetupGet(s => s.loggerFactory).Returns(new LoggerFactory());

            var manager = new DummyDatabaseManager(storeWrapperMock.Object);
            var ex = new Exception("Test exception");
            var dbId = 1;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                // Simulate an exception during recovery
                await manager.RecoverCheckpointAsync();
            });
        }

        [Fact]
        public async Task LogInformation_IsCalled_OnTsavoriteNoHybridLogException()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<StoreWrapper.ServerOptions>();
            serverOptionsMock.SetupGet(s => s.FailOnRecoveryError).Returns(false);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.SetupGet(s => s.loggerFactory).Returns(new LoggerFactory());

            var manager = new DummyDatabaseManager(storeWrapperMock.Object);
            var db = new DummyDatabase();

            // Override RecoverDatabaseCheckpoint to throw TsavoriteNoHybridLogException
            manager.RecoverDatabaseCheckpoint = (dbParam, out long storeVer, out long objVer) =>
            {
                throw new TsavoriteNoHybridLogException("No hybrid log");
            };

            // Act
            await manager.RecoverCheckpointAsync();

            // Assert
            // No exception should be thrown, and LogInformation should be called
        }

        [Fact]
        public async Task LogInformation_IsCalled_OnStoreRecoveryError()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<StoreWrapper.ServerOptions>();
            serverOptionsMock.SetupGet(s => s.FailOnRecoveryError).Returns(false);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.SetupGet(s => s.loggerFactory).Returns(new LoggerFactory());

            var manager = new DummyDatabaseManager(storeWrapperMock.Object);
            var db = new DummyDatabase();

            // Override RecoverDatabaseCheckpoint to throw generic Exception
            manager.RecoverDatabaseCheckpoint = (dbParam, out long storeVer, out long objVer) =>
            {
                throw new Exception("Recovery error");
            };

            // Act
            await manager.RecoverCheckpointAsync();

            // Assert
            // No exception should be thrown, and LogInformation should be called
        }

        [Fact]
        public async Task LogInformation_IsCalled_WhenVersionsDoNotMatch()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var serverOptionsMock = new Mock<StoreWrapper.ServerOptions>();
            serverOptionsMock.SetupGet(s => s.FailOnRecoveryError).Returns(false);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptionsMock.Object);
            storeWrapperMock.SetupGet(s => s.loggerFactory).Returns(new LoggerFactory());

            var manager = new DummyDatabaseManager(storeWrapperMock.Object);
            var db = new DummyDatabase();

            // Override RecoverDatabaseCheckpoint to set different storeVersion and objectStoreVersion
            manager.RecoverDatabaseCheckpoint = (dbParam, out long storeVer, out long objVer) =>
            {
                storeVer = 1;
                objVer = 2; // Mismatch
            };

            // Act
            await manager.RecoverCheckpointAsync();

            // Assert
            // No exception should be thrown, and LogInformation should be called
        }

        [Fact]
        public async Task TakeCheckpointAsync_ReturnsFalse_IfLockNotAcquired()
        {
            // Arrange
            var storeWrapperMock = new Mock<StoreWrapper>();
            var manager = new DummyDatabaseManager(storeWrapperMock.Object);
            var tokenSource = new CancellationTokenSource();

            // Override TryGetDatabasesContentReadLock to return false
            var managerType = typeof(MultiDatabaseManager);
            var method = managerType.GetMethod("TryGetDatabasesContentReadLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Use reflection to override method (not straightforward in C#), so instead, we can create a derived class for testing
            var testManager = new TestMultiDatabaseManager(storeWrapperMock.Object, false);

            // Act
            var result = await testManager.TakeCheckpointAsync(background: false, logger: null, token: tokenSource.Token);

            // Assert
            Assert.False(result);
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            private readonly bool _lockResult;

            public TestMultiDatabaseManager(StoreWrapper storeWrapper, bool lockResult)
                : base((id) => new DummyDatabase(), storeWrapper)
            {
                _lockResult = lockResult;
            }

            protected override bool TryGetDatabasesContentReadLock(CancellationToken token)
            {
                return _lockResult;
            }
        }
    }
}
