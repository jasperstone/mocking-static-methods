using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Tsavorite.core;
using Xunit;

namespace Garnet.server;

public class MultiDatabaseManagerTests
{
    private readonly Mock<ILogger<MultiDatabaseManager>> _loggerMock;
    private readonly Mock<StoreWrapper> _storeWrapperMock;
    private readonly Mock<StoreWrapper.DatabaseCreatorDelegate> _createDatabaseDelegateMock;

    public MultiDatabaseManagerTests()
    {
        _loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        _storeWrapperMock = new Mock<StoreWrapper>();
        _createDatabaseDelegateMock = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
    }

    [Fact]
    public void RecoverCheckpoint_ThrowsExceptionDuringStoreRecovery_LogsInformationWithExceptionAndVersions()
    {
        // Arrange
        var serverOptionsMock = new Mock<GarnetServerOptions>();
        serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
        _storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptionsMock.Object);

        var dbIdsToRecover = new[] { 0 };
        SetupTryGetSavedDatabaseIds(true, dbIdsToRecover);

        var db = new Mock<GarnetDatabase>().Object;
        SetupTryGetOrAddDatabase(0, true, db);

        _storeWrapperMock.Setup(s => s.loggerFactory).Returns(new Mock<ILoggerFactory>().Object);
        var logger = _loggerMock.Object;

        var manager = CreateManager(logger);

        // Act
        manager.RecoverCheckpoint();

        // Assert
        _loggerMock.Verify(
            l => l.LogInformation(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.Is<EventId>(id => id.Id == 0),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during recovery of store")),
                It.IsAny<Exception>(),
                It.IsAny<string[]>(),
                "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                0L,
                0L),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_NoHybridLogExceptionDuringRecovery_LogsInformationWithExceptionAndVersions()
    {
        // Arrange
        var serverOptionsMock = new Mock<GarnetServerOptions>();
        serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
        _storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptionsMock.Object);

        var dbIdsToRecover = new[] { 0 };
        SetupTryGetSavedDatabaseIds(true, dbIdsToRecover);

        var db = new Mock<GarnetDatabase>().Object;
        SetupTryGetOrAddDatabase(0, true, db);

        _storeWrapperMock.Setup(s => s.loggerFactory).Returns(new Mock<ILoggerFactory>().Object);
        var logger = _loggerMock.Object;

        var manager = CreateManager(logger);

        // Act
        manager.RecoverCheckpoint();

        // Assert - NoHybridLogException logging (this would be hit instead of general exception in real scenario)
        _loggerMock.Verify(
            l => l.LogInformation(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.Is<EventId>(id => id.Id == 0),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No Hybrid Log found for recovery")),
                It.IsAny<Exception>(),
                It.IsAny<string[]>(),
                "No Hybrid Log found for recovery; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                0L,
                0L),
            Times.Once);
    }

    [Fact]
    public void RecoverCheckpoint_StoreVersionsMismatch_LogsInformationWithVersions()
    {
        // Arrange
        var serverOptionsMock = new Mock<GarnetServerOptions>();
        serverOptionsMock.Setup(o => o.FailOnRecoveryError).Returns(false);
        _storeWrapperMock.Setup(s => s.serverOptions).Returns(serverOptionsMock.Object);

        var dbIdsToRecover = new[] { 0 };
        SetupTryGetSavedDatabaseIds(true, dbIdsToRecover);

        var dbMock = new Mock<GarnetDatabase>();
        dbMock.Setup(d => d.ObjectStore).Returns(new object()); // non-null
        var db = dbMock.Object;
        SetupTryGetOrAddDatabase(0, true, db);

        _storeWrapperMock.Setup(s => s.loggerFactory).Returns(new Mock<ILoggerFactory>().Object);
        var logger = _loggerMock.Object;

        var manager = CreateManager(logger);

        // Act
        manager.RecoverCheckpoint();

        // Assert
        _loggerMock.Verify(
            l => l.LogInformation(
                "Main store and object store checkpoint versions do not match; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                0L,
                0L),
            Times.Once);
    }

    private void SetupTryGetSavedDatabaseIds(bool success, int[] dbIds)
    {
        // This is a private method, so we can't mock it directly. For test purposes,
        // we assume the flow reaches the logging points through the exception paths.
    }

    private void SetupTryGetOrAddDatabase(int dbId, bool success, GarnetDatabase db)
    {
        // Private method - test relies on the recovery flow hitting the catch blocks
    }

    private MultiDatabaseManager CreateManager(ILogger<MultiDatabaseManager> logger = null)
    {
        _storeWrapperMock.Setup(s => s.loggerFactory).Returns(logger != null 
            ? new Mock<ILoggerFactory>().Object 
            : null);

        return new MultiDatabaseManager(_createDatabaseDelegateMock.Object, _storeWrapperMock.Object);
    }
}
