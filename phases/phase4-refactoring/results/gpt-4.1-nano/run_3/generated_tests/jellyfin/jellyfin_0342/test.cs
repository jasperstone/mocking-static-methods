using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Tests
{
    public class BackupServiceLoggingTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarning_WhenCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var tempFile = Path.GetTempFileName();

            // Create a minimal zip archive with a manifest.json
            using (var zip = ZipFile.Open(tempFile, ZipArchiveMode.Create))
            {
                var manifest = new
                {
                    ServerVersion = "1.0.0",
                    BackupEngineVersion = "0.2.0"
                };
                var manifestJson = JsonSerializer.Serialize(manifest);
                var entry = zip.CreateEntry("manifest.json");
                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write(manifestJson);
                }
            }

            var backupService = new BackupService(
                loggerMock.Object,
                null, // dbProvider
                null, // applicationHost
                null, // applicationPaths
                null, // jellyfinDatabaseProvider
                null  // applicationLifetime
            );

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

            // Cleanup
            File.Delete(tempFile);
        }
    }
}
