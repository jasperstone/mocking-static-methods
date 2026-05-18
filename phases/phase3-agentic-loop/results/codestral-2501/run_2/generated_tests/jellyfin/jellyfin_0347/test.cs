using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
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

            var archivePath = "path/to/archive.zip";

            // Mock the necessary methods and properties
            var dbContextMock = new Mock<JellyfinDbContext>();
            dbProviderMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            var zipArchiveMock = new Mock<ZipArchive>();
            var zipArchiveEntryMock = new Mock<ZipArchiveEntry>();
            zipArchiveMock.Setup(x => x.GetEntry(It.IsAny<string>()))
                .Returns(zipArchiveEntryMock.Object);

            var manifestStreamMock = new Mock<Stream>();
            zipArchiveEntryMock.Setup(x => x.OpenAsync())
                .ReturnsAsync(manifestStreamMock.Object);

            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new BackupOptions { Database = true }
            };

            var manifestJson = JsonSerializer.Serialize(manifest);
            var manifestStream = new MemoryStream();
            var writer = new StreamWriter(manifestStream);
            writer.Write(manifestJson);
            writer.Flush();
            manifestStream.Position = 0;

            manifestStreamMock.Setup(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((byte[] buffer, int count, CancellationToken token) => manifestStream.ReadAsync(buffer, 0, count, token));

            var fileStreamMock = new Mock<FileStream>(archivePath, FileMode.Open);
            fileStreamMock.Setup(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((byte[] buffer, int count, CancellationToken token) => manifestStream.ReadAsync(buffer, 0, count, token));

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Database Purged"),
                Times.Once);
        }
    }
}
