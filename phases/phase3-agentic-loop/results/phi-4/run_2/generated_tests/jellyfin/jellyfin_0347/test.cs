using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BackupServiceTests
{
    [Fact]
    public async Task RestoreBackupAsync_LogsDatabasePurged()
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

        var archivePath = "test.zip";
        var zipArchive = new Mock<ZipArchive>();
        var zipEntry = new Mock<ZipArchiveEntry>();
        zipArchive.Setup(z => z.GetEntry(It.IsAny<string>())).Returns(zipEntry.Object);
        zipEntry.Setup(e => e.OpenAsync()).ReturnsAsync(new MemoryStream());

        using var fileStream = new MemoryStream();
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);
        archive.CreateEntry(BackupService.ManifestEntryName);

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Database Purged"),
            Times.Once);
    }
}
