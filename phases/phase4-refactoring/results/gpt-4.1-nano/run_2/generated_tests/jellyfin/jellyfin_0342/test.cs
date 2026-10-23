using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Tests
{
    // Minimal stub interfaces/classes to compile the test
    public interface IServerApplicationHost
    {
        string RestoreBackupPath { get; set; }
        bool ShouldRestart { get; set; }
        void NotifyPendingRestart();
        Version ApplicationVersion { get; }
    }

    public interface IServerApplicationPaths
    {
        string ConfigurationDirectoryPath { get; }
        string DataPath { get; }
        string RootFolderPath { get; }
        string InternalMetadataPath { get; }
        string DefaultInternalMetadataPath { get; }
    }

    public interface IJellyfinDatabaseProvider { }

    public class JellyfinDbContext : IDisposable
    {
        public void Dispose() { }
    }

    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarningCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                databaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var tempFilePath = Path.GetTempFileName();

            // Create a minimal valid archive with manifest.json
            using (var archiveStream = new FileStream(tempFilePath, FileMode.Create))
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                {
                    var manifest = new
                    {
                        ServerVersion = new Version(1, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0)
                    };
                    var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write(manifestJson);
                    }
                }
            }

            // Act
            await backupService.RestoreBackupAsync(tempFilePath);

            // Assert
            // Verify that LogWarning was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Begin restoring system to {tempFilePath}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Cleanup
            File.Delete(tempFilePath);
        }
    }
}
