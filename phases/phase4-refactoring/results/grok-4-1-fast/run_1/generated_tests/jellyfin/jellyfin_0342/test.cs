using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private const string TestArchivePath = "test-backup.zip";
        private const string ManifestEntryName = "manifest.json";

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningAtStart_FileNotFound()
        {
            // Arrange
            var logger = new Mock<ILogger<BackupService>>();
            var dbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHost = new Mock<IServerApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var applicationLifetime = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                logger.Object,
                dbProvider.Object,
                applicationHost.Object,
                applicationPaths.Object,
                jellyfinDatabaseProvider.Object,
                applicationLifetime.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(
                () => backupService.RestoreBackupAsync(TestArchivePath));
            
            Assert.Contains("does not exist", ex.Message);

            // Verify LogWarning was called FIRST, before the exception
            logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Begin restoring system to") && v.ToString()!.Contains(TestArchivePath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ScheduleRestoreAndRestartServer_SetsPropertiesAndSchedulesShutdown()
        {
            // Arrange
            var logger = new Mock<ILogger<BackupService>>();
            var dbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHost = new Mock<IServerApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var applicationLifetime = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                logger.Object,
                dbProvider.Object,
                applicationHost.Object,
                applicationPaths.Object,
                jellyfinDatabaseProvider.Object,
                applicationLifetime.Object);

            // Act
            backupService.ScheduleRestoreAndRestartServer(TestArchivePath);

            // Assert
            applicationHost.VerifySet(h => h.RestoreBackupPath = TestArchivePath);
            applicationHost.VerifySet(h => h.ShouldRestart = true);
            applicationHost.Verify(h => h.NotifyPendingRestart(), Times.Once);
            applicationLifetime.Verify(l => l.StopApplication(), Times.Never); // Called async after delay
        }
    }

    // Test-specific minimal implementations
    public class BackupManifest
    {
        public string ServerVersion { get; set; } = string.Empty;
        public string BackupEngineVersion { get; set; } = string.Empty;
    }
}
