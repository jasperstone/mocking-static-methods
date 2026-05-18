using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Hosting;
using MediaBrowser.Controller;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller.SystemBackupService;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private interface IDbContextFactory<T> where T : class
        {
            Task<T> CreateDbContextAsync();
        }

        private class DummyDbContext { }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWithArchivePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<DummyDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            applicationHostMock.SetupGet(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            var testArchivePath = "testArchive.zip";

            // Create a minimal valid zip archive with manifest.json entry asynchronously
            await using (var fs = File.Create(testArchivePath))
            await using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, true))
            {
                var manifestEntry = zip.CreateEntry("manifest.json");
                await using var entryStream = manifestEntry.Open();
                var manifest = new
                {
                    ServerVersion = "1.0.0",
                    BackupEngineVersion = "0.2.0",
                    Options = new { Database = false }
                };
                await JsonSerializer.SerializeAsync(entryStream, manifest);
            }

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object as IDbContextFactory<JellyfinDbContext>,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // Act
            await backupService.RestoreBackupAsync(testArchivePath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to") && v.ToString().Contains(testArchivePath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Cleanup
            File.Delete(testArchivePath);
        }
    }
}
