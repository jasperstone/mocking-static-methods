using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;
using Jellyfin.Database.Implementations;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
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
            var fileStream = new MemoryStream();
            var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true);
            var manifestEntry = zipArchive.CreateEntry("manifest.json");
            using (var entryStream = manifestEntry.Open())
            {
                await JsonSerializer.SerializeAsync(entryStream, new BackupManifest { ServerVersion = new Version(1, 0, 0), BackupEngineVersion = new Version(0, 2, 0), Options = new BackupOptions { Database = true } });
            }
            zipArchive.Dispose();
            fileStream.Position = 0;

            var dbContext = new Mock<JellyfinDbContext>();
            _mockDbProvider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext.Object);

            var historyEntry = zipArchive.CreateEntry("Database/HistoryRow.json");
            using (var entryStream = historyEntry.Open())
            {
                await JsonSerializer.SerializeAsync(entryStream, new HistoryRow[] { new HistoryRow("1", "1.0.0") });
            }

            var entityTypes = typeof(JellyfinDbContext).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(e => e.PropertyType.IsAssignableTo(typeof(IQueryable)))
                .Select(e => (Type: e, Set: e.GetValue(dbContext.Object) as IQueryable))
                .ToArray();

            var tableNames = entityTypes.Select(f => dbContext.Object.Model.FindEntityType(f.Type.PropertyType.GetGenericArguments()[0])!.GetSchemaQualifiedTableName()!);

            _mockJellyfinDatabaseProvider.Setup(x => x.PurgeDatabase(dbContext.Object, tableNames)).Returns(Task.CompletedTask);

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
        }
    }
}
