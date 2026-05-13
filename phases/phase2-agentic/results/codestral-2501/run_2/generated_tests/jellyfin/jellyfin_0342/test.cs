using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _jellyfinDatabaseProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                _jellyfinDatabaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarning()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var manifestJson = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"DateCreated\": \"2023-01-01T00:00:00Z\", \"DatabaseTables\": [], \"Options\": {\"Database\": true}}";
            var manifestEntry = new Mock<ZipArchiveEntry>();
            manifestEntry.Setup(e => e.OpenAsync()).ReturnsAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(manifestJson)));

            var zipArchiveMock = new Mock<ZipArchive>(MockBehavior.Strict);
            zipArchiveMock.Setup(za => za.GetEntry(It.IsAny<string>())).Returns(manifestEntry.Object);

            var fileStreamMock = new Mock<FileStream>(MockBehavior.Strict);
            fileStreamMock.Setup(fs => fs.DisposeAsync()).Returns(ValueTask.CompletedTask);

            using (var zipArchive = zipArchiveMock.Object)
            {
                // Act
                await _backupService.RestoreBackupAsync(archivePath);

                // Assert
                _loggerMock.Verify(
                    x => x.LogWarning("Begin restoring system to {BackupArchive}", It.IsAny<object[]>()),
                    Times.Once);
            }
        }
    }
}
