using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Tests
{
    public class BackupServiceLoggingTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsRestoreAndOverrideMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var lifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbFactoryMock.Object,
                appHostMock.Object,
                appPathsMock.Object,
                jellyfinProviderMock.Object,
                lifetimeMock.Object);

            // Create a dummy zip archive with manifest.json and a file to trigger logs
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var zipStream = new MemoryStream())
                {
                    using (var archive = new System.IO.Compression.ZipArchive(zipStream, ZipArchiveMode.Create, true))
                    {
                        var manifestEntry = archive.CreateEntry("manifest.json");
                        using (var writer = new StreamWriter(manifestEntry.Open()))
                        {
                            writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\"}");
                        }
                        var fileEntry = archive.CreateEntry("Config/testfile.txt");
                        using (var writer = new StreamWriter(fileEntry.Open()))
                        {
                            writer.Write("test");
                        }
                    }
                    File.WriteAllBytes(tempFile, zipStream.ToArray());
                }

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Restore and override")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
