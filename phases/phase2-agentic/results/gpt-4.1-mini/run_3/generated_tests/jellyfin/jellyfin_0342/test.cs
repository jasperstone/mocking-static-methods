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

            // Setup a temporary file to simulate the archive
            var tempFile = Path.GetTempFileName();
            try
            {
                // Create a minimal valid zip archive with manifest.json entry
                using (var fs = File.OpenWrite(tempFile))
                using (var archive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using var entryStream = manifestEntry.Open();
                    using var writer = new StreamWriter(entryStream);
                    // Write minimal valid manifest json with ServerVersion <= applicationHost.ApplicationVersion
                    writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":false}}");
                }

                // Setup applicationHost.ApplicationVersion to 1.0.0
                applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

                // Setup File.Exists to true for the temp file path
                // We cannot mock static File.Exists, so we rely on the actual file existing (which it does)

                // Setup StorageHelper.TestCommonPathsForStorageCapacity to do nothing
                // This is a static method, so we cannot mock it easily; assume it does not throw

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
