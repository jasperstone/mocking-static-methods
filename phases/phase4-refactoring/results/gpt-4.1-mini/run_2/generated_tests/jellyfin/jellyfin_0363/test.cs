using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsBackupOfFolderConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            // Setup application paths
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            applicationPathsMock.Setup(ap => ap.ConfigurationDirectoryPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.DataPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.RootFolderPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.InternalMetadataPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.DefaultInternalMetadataPath).Returns(tempDir);

            // Setup application host version to be compatible
            applicationHostMock.SetupGet(ah => ah.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Create dummy manifest content
            var manifest = new
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new { Database = false }
            };

            // Create a dummy zip archive with manifest.json entry
            var dummyZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            using (var zip = ZipFile.Open(dummyZipPath, ZipArchiveMode.Create))
            {
                var manifestEntry = zip.CreateEntry("manifest.json");
                using var writer = new StreamWriter(manifestEntry.Open());
                var json = JsonSerializer.Serialize(manifest);
                writer.Write(json);
            }

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // Act
            await backupService.RestoreBackupAsync(dummyZipPath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            File.Delete(dummyZipPath);
            Directory.Delete(tempDir, true);
        }
    }
}
