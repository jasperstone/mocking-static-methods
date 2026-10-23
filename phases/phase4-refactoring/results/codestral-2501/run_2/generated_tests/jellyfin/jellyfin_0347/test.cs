using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbProvider;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IJellyfinDatabaseProvider> _mockJellyfinDatabaseProvider;
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _mockLogger.Object,
                _mockDbProvider.Object,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                _mockJellyfinDatabaseProvider.Object,
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var manifestEntryName = "manifest.json";
            var manifestJson = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"Options\": {\"Database\": true}}";
            var historyJson = "[]";
            var entityJson = "[]";

            var fileStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                var manifestEntry = zipArchive.CreateEntry(manifestEntryName);
                using (var writer = new StreamWriter(await manifestEntry.OpenAsync()))
                {
                    await writer.WriteAsync(manifestJson);
                }

                var historyEntry = zipArchive.CreateEntry("Database/HistoryRow.json");
                using (var writer = new StreamWriter(await historyEntry.OpenAsync()))
                {
                    await writer.WriteAsync(historyJson);
                }

                var entityEntry = zipArchive.CreateEntry("Database/Entity.json");
                using (var writer = new StreamWriter(await entityEntry.OpenAsync()))
                {
                    await writer.WriteAsync(entityJson);
                }
            }

            fileStream.Position = 0;
            var mockFileStream = new Mock<FileStream>(fileStream, FileAccess.Read);
            mockFileStream.Setup(fs => fs.Name).Returns(archivePath);

            var mockDbContext = new Mock<JellyfinDbContext>();
            _mockDbProvider.Setup(db => db.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockDbContext.Object);

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
