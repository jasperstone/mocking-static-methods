using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
            var manifestJson = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"Options\": {\"Database\": true}}";
            var historyJson = "[{\"MigrationId\": \"1\", \"ProductVersion\": \"1.0.0\"}]";
            var entityJson = "[{\"Id\": 1, \"Name\": \"Entity1\"}]";

            var zipArchive = new Mock<ZipArchive>();
            var zipArchiveEntry = new Mock<ZipArchiveEntry>();
            var manifestStream = new MemoryStream();
            var historyStream = new MemoryStream();
            var entityStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(manifestStream, new BackupManifest { ServerVersion = new Version(1, 0, 0), BackupEngineVersion = new Version(1, 0, 0), Options = new BackupOptions { Database = true } });
            await JsonSerializer.SerializeAsync(historyStream, new HistoryRow[] { new HistoryRow("1", "1.0.0") });
            await JsonSerializer.SerializeAsync(entityStream, new JsonObject[] { new JsonObject { { "Id", 1 }, { "Name", "Entity1" } } });

            manifestStream.Position = 0;
            historyStream.Position = 0;
            entityStream.Position = 0;

            zipArchiveEntry.Setup(e => e.OpenAsync()).ReturnsAsync(manifestStream);
            zipArchive.Setup(a => a.GetEntry("manifest.json")).Returns(zipArchiveEntry.Object);
            zipArchive.Setup(a => a.GetEntry("Database/HistoryRow.json")).Returns(zipArchiveEntry.Object);
            zipArchive.Setup(a => a.GetEntry("Database/EntityType.json")).Returns(zipArchiveEntry.Object);

            var fileStream = new MemoryStream();
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntryFromFile("manifest.json", "manifest.json");
                archive.CreateEntryFromFile("HistoryRow.json", "Database/HistoryRow.json");
                archive.CreateEntryFromFile("EntityType.json", "Database/EntityType.json");
            }
            fileStream.Position = 0;

            var dbContext = new Mock<JellyfinDbContext>();
            var historyRepository = new Mock<IHistoryRepository>();
            var entityType = new Mock<IQueryable>();

            dbContext.Setup(d => d.GetService<IHistoryRepository>()).Returns(historyRepository.Object);
            dbContext.Setup(d => d.Database.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            dbContext.Setup(d => d.Model.FindEntityType(It.IsAny<Type>()).GetSchemaQualifiedTableName()).Returns("EntityType");

            _mockDbProvider.Setup(d => d.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext.Object);
            _mockApplicationHost.Setup(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));
            _mockApplicationPaths.Setup(p => p.ConfigurationDirectoryPath).Returns("Config");
            _mockApplicationPaths.Setup(p => p.DataPath).Returns("Data");
            _mockApplicationPaths.Setup(p => p.RootFolderPath).Returns("Root");
            _mockApplicationPaths.Setup(p => p.InternalMetadataPath).Returns("Data/metadata");
            _mockApplicationPaths.Setup(p => p.DefaultInternalMetadataPath).Returns("Data/metadata-default");

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
