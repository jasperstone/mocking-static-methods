using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private class BackupOptionsStub
        {
            public bool Database { get; set; }
        }

        private class BackupManifestStub
        {
            public Version ServerVersion { get; set; }
            public Version BackupEngineVersion { get; set; }
            public BackupOptionsStub Options { get; set; }
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<object>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<object>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                (IJellyfinDatabaseProvider)jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                // Create a minimal valid zip archive with manifest.json entry to avoid exceptions after logging
                using (var fs = File.Open(tempFile, FileMode.Open, FileAccess.ReadWrite))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Update, true))
                {
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    await using (var entryStream = manifestEntry.Open())
                    {
                        var manifest = new BackupManifestStub
                        {
                            ServerVersion = new Version(1, 0, 0),
                            BackupEngineVersion = new Version(0, 2, 0),
                            Options = new BackupOptionsStub { Database = false }
                        };
                        await JsonSerializer.SerializeAsync(entryStream, manifest);
                    }
                }

                // Setup applicationHost.ApplicationVersion to 1.0.0 to pass version check
                applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

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
