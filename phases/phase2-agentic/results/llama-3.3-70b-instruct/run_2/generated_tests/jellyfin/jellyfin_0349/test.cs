using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup;

public class BackupServiceTests
{
    private readonly Mock<ILogger<BackupService>> _loggerMock;
    private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
    private readonly Mock<IServerApplicationHost> _applicationHostMock;
    private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
    private readonly Mock<IJellyfinDatabaseProvider> _jellyfinDatabaseProviderMock;
    private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;

    public BackupServiceTests()
    {
        _loggerMock = new Mock<ILogger<BackupService>>();
        _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        _applicationHostMock = new Mock<IServerApplicationHost>();
        _applicationPathsMock = new Mock<IServerApplicationPaths>();
        _jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled_WhenNoBackupOfExpectedTableIsPresent()
    {
        // Arrange
        var backupService = new BackupService(
            _loggerMock.Object,
            _dbProviderMock.Object,
            _applicationHostMock.Object,
            _applicationPathsMock.Object,
            _jellyfinDatabaseProviderMock.Object,
            _hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup/archive.zip";
        var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Read, false);
        var zipEntry = zipArchive.CreateEntry("Database/HistoryRow.json");

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        _loggerMock.Verify(logger => logger.LogInformation("No backup of expected table {Table} is present in backup, continuing anyway", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled_WhenBeginPurgingDatabase()
    {
        // Arrange
        var backupService = new BackupService(
            _loggerMock.Object,
            _dbProviderMock.Object,
            _applicationHostMock.Object,
            _applicationPathsMock.Object,
            _jellyfinDatabaseProviderMock.Object,
            _hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup/archive.zip";
        var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Read, false);
        var zipEntry = zipArchive.CreateEntry("Database/HistoryRow.json");

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        _loggerMock.Verify(logger => logger.LogInformation("Begin purging database"), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled_WhenDatabasePurged()
    {
        // Arrange
        var backupService = new BackupService(
            _loggerMock.Object,
            _dbProviderMock.Object,
            _applicationHostMock.Object,
            _applicationPathsMock.Object,
            _jellyfinDatabaseProviderMock.Object,
            _hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup/archive.zip";
        var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Read, false);
        var zipEntry = zipArchive.CreateEntry("Database/HistoryRow.json");

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        _loggerMock.Verify(logger => logger.LogInformation("Database Purged"), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled_WhenReadBackupOfTable()
    {
        // Arrange
        var backupService = new BackupService(
            _loggerMock.Object,
            _dbProviderMock.Object,
            _applicationHostMock.Object,
            _applicationPathsMock.Object,
            _jellyfinDatabaseProviderMock.Object,
            _hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup/archive.zip";
        var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Read, false);
        var zipEntry = zipArchive.CreateEntry("Database/HistoryRow.json");

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        _loggerMock.Verify(logger => logger.LogInformation("Read backup of {Table}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled_WhenRestoreBackupOfTable()
    {
        // Arrange
        var backupService = new BackupService(
            _loggerMock.Object,
            _dbProviderMock.Object,
            _applicationHostMock.Object,
            _applicationPathsMock.Object,
            _jellyfinDatabaseProviderMock.Object,
            _hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup/archive.zip";
        var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Read, false);
        var zipEntry = zipArchive.CreateEntry("Database/HistoryRow.json");

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        _loggerMock.Verify(logger => logger.LogInformation("Restore backup of {Table}", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled_WhenPreparedToRestoreEntriesForTable()
    {
        // Arrange
        var backupService = new BackupService(
            _loggerMock.Object,
            _dbProviderMock.Object,
            _applicationHostMock.Object,
            _applicationPathsMock.Object,
            _jellyfinDatabaseProviderMock.Object,
            _hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup/archive.zip";
        var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Read, false);
        var zipEntry = zipArchive.CreateEntry("Database/HistoryRow.json");

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        _loggerMock.Verify(logger => logger.LogInformation("Prepared to restore {Number} entries for {Table}", It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }
}
