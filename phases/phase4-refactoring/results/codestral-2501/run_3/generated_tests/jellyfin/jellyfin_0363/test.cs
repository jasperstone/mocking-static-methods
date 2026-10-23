using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbProvider;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IJellyfinDatabaseProvider> _mockJellyfinDatabaseProvider;
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _mockLogger.Object,
                _mockDbProvider.Object,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                _mockJellyfinDatabaseProvider.Object,
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsInformation()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var manifestEntryName = "manifest.json";
            var manifestContent = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"Options\": {\"Database\": true}}";
            var manifestStream = new MemoryStream();
            var writer = new Utf8JsonWriter(manifestStream);
            JsonSerializer.Serialize(writer, JsonDocument.Parse(manifestContent).RootElement);
            writer.Flush();
            manifestStream.Position = 0;

            var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            var manifestEntry = zipArchive.CreateEntry(manifestEntryName);
            using (var entryStream = manifestEntry.Open())
            {
                await manifestStream.CopyToAsync(entryStream);
            }
            zipArchive.Dispose();

            _mockApplicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));
            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("path/to/config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("path/to/data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("path/to/root");
            _mockApplicationPaths.Setup(x => x.InternalMetadataPath).Returns("path/to/internal/metadata");
            _mockApplicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("path/to/default/internal/metadata");

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
