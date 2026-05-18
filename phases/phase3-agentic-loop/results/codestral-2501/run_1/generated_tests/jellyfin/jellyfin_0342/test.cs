using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _jellyfinDatabaseProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarning_WhenBackupFileExists()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                _jellyfinDatabaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object);

            // Mock file existence
            File.Create(archivePath).Close();

            // Mock zip archive
            var zipArchiveMock = new Mock<ZipArchive>(File.OpenRead(archivePath), ZipArchiveMode.Read, false);
            var zipArchiveEntryMock = new Mock<ZipArchiveEntry>();
            zipArchiveMock.Setup(x => x.GetEntry(It.IsAny<string>())).Returns(zipArchiveEntryMock.Object);

            // Mock manifest
            var manifest = new BackupManifest { ServerVersion = new Version(1, 0, 0) };
            var manifestStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(manifestStream, manifest, new JsonSerializerOptions { WriteIndented = true });
            manifestStream.Position = 0;
            zipArchiveEntryMock.Setup(x => x.OpenAsync()).ReturnsAsync(manifestStream);

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            // Clean up
            File.Delete(archivePath);
        }
    }
}
