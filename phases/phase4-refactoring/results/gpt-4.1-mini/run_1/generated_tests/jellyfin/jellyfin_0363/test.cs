using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
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

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsBackupOfFolderConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup ConfigurationDirectoryPath to a temp directory with some files
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            applicationPathsMock.Setup(ap => ap.ConfigurationDirectoryPath).Returns(tempDir);

            // Create some dummy config files asynchronously
            var xmlFile = Path.Combine(tempDir, "config1.xml");
            var jsonFile = Path.Combine(tempDir, "config2.json");
            await File.WriteAllTextAsync(xmlFile, "<xml></xml>");
            await File.WriteAllTextAsync(jsonFile, "{}");

            // Setup ApplicationVersion to a lower version to pass version check
            applicationHostMock.SetupGet(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Create a dummy manifest json content
            var manifestJson = "{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":false}}";

            // Create a dummy zip archive file with manifest.json entry to satisfy the method
            var archivePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            using (var fs = new FileStream(archivePath, FileMode.Create))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using var entryStream = await manifestEntry.OpenAsync(CancellationToken.None);
                using var writer = new StreamWriter(entryStream);
                await writer.WriteAsync(manifestJson);
            }

            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder") && v.ToString().Contains(tempDir)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            File.Delete(archivePath);
            File.Delete(xmlFile);
            File.Delete(jsonFile);
            Directory.Delete(tempDir);
        }
    }
}
