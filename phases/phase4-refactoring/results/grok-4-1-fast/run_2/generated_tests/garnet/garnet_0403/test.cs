using System;
using System.IO;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Garnet.server;
using Garnet.common;
using Tsavorite.core;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerLoggerTests
    {
        private readonly Mock<ILogger<MultiDatabaseManager>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly Mock<StoreWrapper.DatabaseCreatorDelegate> _createDatabaseDelegateMock;
        private readonly Mock<ServerOptions> _serverOptionsMock;

        public MultiDatabaseManagerLoggerTests()
        {
            _loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            
            _storeWrapperMock = new Mock<StoreWrapper>();
            _createDatabaseDelegateMock = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
            _serverOptionsMock = new Mock<ServerOptions>();
            _serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
            _serverOptionsMock.Setup(o => o.MainStoreCheckpointBaseDirectory).Returns("/tmp/checkpoint");
            _serverOptionsMock.Setup(o => o.GetCheckpointDirectoryName(It.IsAny<int>())).Returns("checkpoint-0");
            _storeWrapperMock.Setup(s => s.serverOptions).Returns(_serverOptionsMock.Object);
            _storeWrapperMock.Setup(s => s.loggerFactory).Returns(_loggerFactoryMock.Object);
        }

        [Fact]
        public void RecoverCheckpoint_LogsErrorDuringDatabaseIdsRecovery()
        {
            // Arrange - Setup to throw in TryGetSavedDatabaseIds
            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object, createDefaultDatabase: false);

            // Act
            manager.RecoverCheckpoint();

            // Assert - Verifies Logger?.LogInformation(ex, "Error during recovery of database ids...") call
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyFormat>(f => f.ToString().Contains("Error during recovery of database ids")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void RecoverCheckpoint_LogsNoHybridLogException()
        {
            // Arrange - Setup database creator to throw TsavoriteNoHybridLogException in RecoverDatabaseCheckpoint
            _createDatabaseDelegateMock.Setup(d => d(It.IsAny<int>()))
                .Returns(() =>
                {
                    var dbMock = new Mock<GarnetDatabase>();
                    dbMock.Setup(d => d.VectorManager).Returns(new Mock<IVectorManager>().Object);
                    return dbMock.Object;
                });

            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);

            // Act
            manager.RecoverCheckpoint();

            // Assert - Verifies Logger?.LogInformation(ex, "No Hybrid Log found for recovery...") call
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyFormat>(f => f.ToString().Contains("No Hybrid Log found for recovery")),
                    It.IsAny<TsavoriteNoHybridLogException>(),
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public void RecoverCheckpoint_LogsGenericExceptionDuringStoreRecovery()
        {
            // Arrange - Setup to throw generic exception during recovery (line 137 target)
            _createDatabaseDelegateMock.Setup(d => d(It.IsAny<int>()))
                .Returns(() =>
                {
                    var dbMock = new Mock<GarnetDatabase>();
                    dbMock.Setup(d => d.MainStore).Throws(new InvalidOperationException("Recovery failed"));
                    dbMock.Setup(d => d.ObjectStore).Returns((TsavoriteKV<byte[], IGarnetObject<object>, object, object>)null);
                    dbMock.Setup(d => d.VectorManager).Returns(new Mock<IVectorManager>().Object);
                    return dbMock.Object;
                });

            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);

            // Act
            manager.RecoverCheckpoint();

            // Assert - Verifies Logger?.LogInformation(ex, "Error during recovery of store...") call at line 137
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyFormat>(f => f.ToString().Contains("Error during recovery of store")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public void RecoverCheckpoint_LogsStoreVersionMismatch()
        {
            // Arrange - Setup version mismatch scenario
            _createDatabaseDelegateMock.Setup(d => d(It.IsAny<int>()))
                .Returns(() =>
                {
                    var dbMock = new Mock<GarnetDatabase>();
                    var mainStoreMock = new Mock<TsavoriteKV<SpanByte, SpanByte, object, object>>();
                    mainStoreMock.Setup(s => s.GetRecoverVersion()).Returns(10L);
                    mainStoreMock.Setup(s => s.Recover(It.IsAny<long>())).Returns(10L);

                    var objStoreMock = new Mock<TsavoriteKV<byte[], IGarnetObject<object>, object, object>>();
                    objStoreMock.Setup(s => s.GetRecoverVersion()).Returns(5L);
                    objStoreMock.Setup(s => s.Recover(It.IsAny<long>())).Returns(5L);

                    dbMock.SetupGet(d => d.MainStore).Returns(mainStoreMock.Object);
                    dbMock.SetupGet(d => d.ObjectStore).Returns(objStoreMock.Object);
                    dbMock.Setup(d => d.VectorManager).Returns(new Mock<IVectorManager>().Object);
                    return dbMock.Object;
                });

            var manager = new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);

            // Act
            manager.RecoverCheckpoint();

            // Assert - Verifies Logger?.LogInformation("Main store and object store checkpoint versions do not match...")
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyFormat>(f => f.ToString().Contains("Main store and object store checkpoint versions do not match")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat, Exception?, string>>()
                ),
                Times.Once
            );
        }
    }
}
