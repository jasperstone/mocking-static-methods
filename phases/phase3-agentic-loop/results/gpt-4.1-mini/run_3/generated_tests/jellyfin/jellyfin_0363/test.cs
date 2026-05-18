using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.Extensions.Hosting;

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

            // Setup ConfigurationDirectoryPath to a temp directory
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            applicationPathsMock.SetupGet(x => x.ConfigurationDirectoryPath).Returns(tempDir);

            // Create some dummy config files to be enumerated
            var xmlFile = Path.Combine(tempDir, "config1.xml");
            var jsonFile = Path.Combine(tempDir, "config2.json");
            File.WriteAllText(xmlFile, "<xml></xml>");
            File.WriteAllText(jsonFile, "{}");

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // We will call the internal method indirectly by invoking RestoreBackupAsync with a dummy archive path.
            // Since the method reads the archive and expects a manifest.json entry, it will throw.
            // We catch the exception but verify the log call before the exception.

            var dummyArchivePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            File.WriteAllBytes(dummyArchivePath, Array.Empty<byte>());

            // Act
            try
            {
                await backupService.RestoreBackupAsync(dummyArchivePath);
            }
            catch
            {
                // ignored
            }

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
            File.Delete(xmlFile);
            File.Delete(jsonFile);
            Directory.Delete(tempDir);
            File.Delete(dummyArchivePath);
        }
    }
}
