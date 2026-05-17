using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations;

namespace Jellyfin.Tests
{
    public class BackupServiceLoggingTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _dbProviderMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;

        public BackupServiceLoggingTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task RestoreBackupAsync_ShouldLogBackupFolder()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _pathsMock.Object,
                _dbProviderMock.Object,
                _lifetimeMock.Object);

            // Setup minimal mocks to allow calling the method
            var testArchivePath = "test.zip";

            // Create a dummy zip archive with minimal structure
            using var memStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                // Add manifest.json
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var entryStream = manifestEntry.Open())
                {
                    var manifest = new { ServerVersion = "1.0.0", BackupEngineVersion = "0.2.0" };
                    JsonSerializer.Serialize(entryStream, manifest);
                }
            }
            memStream.Seek(0, SeekOrigin.Begin);
            File.WriteAllBytes(testArchivePath, memStream.ToArray());

            // Act
            await backupService.RestoreBackupAsync(testArchivePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
