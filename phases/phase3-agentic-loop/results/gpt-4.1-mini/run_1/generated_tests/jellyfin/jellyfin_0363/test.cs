using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.Extensions.Hosting;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsBackupOfFolderConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup ConfigurationDirectoryPath to a temp directory with some files
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            applicationPathsMock.Setup(ap => ap.ConfigurationDirectoryPath).Returns(tempDir);

            // Create some dummy files to be enumerated
            var xmlFile = Path.Combine(tempDir, "file1.xml");
            var jsonFile = Path.Combine(tempDir, "file2.json");
            await File.WriteAllTextAsync(xmlFile, "<xml></xml>");
            await File.WriteAllTextAsync(jsonFile, "{}");

            // Create a minimal zip archive with manifest.json to allow RestoreBackupAsync to proceed to the point of logging
            var manifest = new
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                DateCreated = DateTimeOffset.UtcNow,
                DatabaseTables = Array.Empty<string>(),
                Options = new { Database = false, Metadata = false, Trickplay = false, Subtitles = false }
            };

            var zipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            using (var fs = new FileStream(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, true))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using var entryStream = manifestEntry.Open();
                await JsonSerializer.SerializeAsync(entryStream, manifest);
            }

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // Act
            await backupService.RestoreBackupAsync(zipPath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            File.Delete(zipPath);
            Directory.Delete(tempDir, true);
        }
    }
}
