using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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

namespace Jellyfin.Server.Implementations.FullSystemBackup;

public class BackupServiceTests
{
    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup.zip";
        var fileStream = new MemoryStream();
        var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalledForPurgeDatabase()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup.zip";
        var fileStream = new MemoryStream();
        var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(l => l.LogInformation("Begin purging database"), Times.Once);
        loggerMock.Verify(l => l.LogInformation("Database Purged"), Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalledForRestoreBackup()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup.zip";
        var fileStream = new MemoryStream();
        var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Read backup of"))), Times.AtLeastOnce);
        loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Restore backup of"))), Times.AtLeastOnce);
        loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Prepared to restore"))), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RestoreBackupAsync_LogInformationCalledForNoBackupOfExpectedTable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            hostApplicationLifetimeMock.Object);

        var archivePath = "path/to/backup.zip";
        var fileStream = new MemoryStream();
        var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("No backup of expected table"))), Times.AtLeastOnce);
    }
}
