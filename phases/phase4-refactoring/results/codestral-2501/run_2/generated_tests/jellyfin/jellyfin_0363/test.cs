using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using System;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;

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
            var manifest = JsonSerializer.Deserialize<BackupManifest>(manifestContent);

            var mockFileStream = new Mock<FileStream>(archivePath, FileMode.Open);
            var mockZipArchive = new Mock<ZipArchive>(mockFileStream.Object, ZipArchiveMode.Read);
            var mockZipArchiveEntry = new Mock<ZipArchiveEntry>();
            var mockManifestStream = new Mock<Stream>();

            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("path/to/config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("path/to/data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("path/to/root");
            _mockApplicationPaths.Setup(x => x.InternalMetadataPath).Returns("path/to/internal/metadata");
            _mockApplicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("path/to/default/internal/metadata");

            _mockApplicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            mockZipArchiveEntry.Setup(x => x.Open()).Returns(mockManifestStream.Object);
            mockZipArchive.Setup(x => x.GetEntry(manifestEntryName)).Returns(mockZipArchiveEntry.Object);
            mockManifestStream.Setup(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(manifestContent.Length));

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
