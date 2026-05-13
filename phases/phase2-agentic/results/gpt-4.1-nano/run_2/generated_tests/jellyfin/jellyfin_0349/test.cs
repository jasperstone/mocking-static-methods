using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _dbProviderMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task LogInformation_IsCalled_DuringRestoreBackupAsync()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _pathsMock.Object,
                _dbProviderMock.Object,
                _lifetimeMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                // Create a minimal valid zip archive with manifest.json and a dummy database
                using (var zipStream = new FileStream(tempFile, FileMode.Create))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Update))
                {
                    // Add manifest.json
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    var manifestContent = new BackupManifest
                    {
                        ServerVersion = new Version(10, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0),
                        Options = new BackupOptions { Database = true }
                    };
                    await using (var entryStream = manifestEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(entryStream, manifestContent);
                    }

                    // Add HistoryRow.json
                    var historyEntry = archive.CreateEntry("Database/HistoryRow.json");
                    await using (var entryStream = historyEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(entryStream, new { MigrationId = "20210101" });
                    }
                }

                // Mock dependencies
                var dbContextMock = new Mock<JellyfinDbContext>();
                _dbFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);
                dbContextMock.Setup(db => db.Database.ExecuteSqlRawAsync(It.IsAny<string>())).ReturnsAsync(1);
                dbContextMock.Setup(db => db.ChangeTracker.QueryTrackingBehavior).Returns(QueryTrackingBehavior.NoTracking);
                _dbProviderMock.Setup(p => p.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Restore and override")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }

    // Minimal placeholder classes for dependencies
    public class JellyfinDbContext : DbContext
    {
        public ChangeTracker ChangeTracker => base.ChangeTracker;
        public DatabaseFacade Database => base.Database;
        public DbSet<HistoryRow> HistoryRows { get; set; }
    }

    public class HistoryRow
    {
        public string MigrationId { get; set; }
    }

    public class BackupManifest
    {
        public Version ServerVersion { get; set; }
        public Version BackupEngineVersion { get; set; }
        public BackupOptions Options { get; set; }
    }

    public class BackupOptions
    {
        public bool Database { get; set; }
    }
}
