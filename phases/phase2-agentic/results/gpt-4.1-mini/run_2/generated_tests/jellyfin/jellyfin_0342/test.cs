using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // Create a temporary empty file to simulate the archive file
            var tempFile = Path.GetTempFileName();

            try
            {
                // Setup File.Exists to return true for the temp file path
                // We cannot mock static File.Exists easily, so we create the file physically

                // Setup minimal valid zip archive with manifest.json entry to avoid exceptions after logging
                using (var fs = File.OpenWrite(tempFile))
                using (var archive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using var entryStream = manifestEntry.Open();
                    using var writer = new StreamWriter(entryStream);
                    // Write minimal valid manifest json with ServerVersion and BackupEngineVersion
                    writer.Write("{\"ServerVersion\":\"1.0.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":false}}");
                }

                // Setup applicationHost.ApplicationVersion to 1.0.0.0 to pass version check
                applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0, 0));

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
