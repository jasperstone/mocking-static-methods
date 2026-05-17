using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
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

            applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Create a minimal valid zip archive with manifest.json entry
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, true))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        var manifestJson = "{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":false}}";
                        writer.Write(manifestJson);
                    }
                }

                var backupService = new BackupService(
                    loggerMock.Object,
                    dbProviderMock.Object,
                    applicationHostMock.Object,
                    applicationPathsMock.Object,
                    jellyfinDatabaseProviderMock.Object,
                    hostApplicationLifetimeMock.Object);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
