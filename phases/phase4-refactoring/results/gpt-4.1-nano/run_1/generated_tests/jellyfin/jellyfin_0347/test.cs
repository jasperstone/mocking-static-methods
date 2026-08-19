using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);

            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostLifetimeMock.Object);

            // Create a minimal ZIP archive with manifest and database entries
            var tempZipPath = Path.GetTempFileName();
            using (var zipStream = File.Create(tempZipPath))
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // Add manifest.json
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    {
                        var manifest = new
                        {
                            ServerVersion = "1.0.0",
                            BackupEngineVersion = "0.2.0",
                            Options = new { Database = true }
                        };
                        JsonSerializer.Serialize(entryStream, manifest);
                    }

                    // Add Database/HistoryRow.json
                    var dbEntry = archive.CreateEntry("Database/HistoryRow.json");
                    using (var entryStream = dbEntry.Open())
                    {
                        JsonSerializer.Serialize(entryStream, new[] { new { MigrationId = "Migration1" } });
                    }
                }
            }

            // Act
            await backupService.RestoreBackupAsync(tempZipPath);

            // Assert
            // Verify that LogInformation("Database Purged") was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            File.Delete(tempZipPath);
        }
    }
}
