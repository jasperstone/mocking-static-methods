using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
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
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var dbContext = new Mock<JellyfinDbContext>();
            var zipArchive = new Mock<ZipArchive>(Stream.Null, ZipArchiveMode.Read, false);
            var zipArchiveEntry = new Mock<ZipArchiveEntry>();
            var manifestStream = new MemoryStream();
            var manifest = new BackupManifest { Options = new BackupOptions { Database = true } };

            _mockDbProvider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext.Object);
            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("root");
            _mockApplicationPaths.Setup(x => x.InternalMetadataPath).Returns("metadata");
            _mockApplicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("metadata-default");

            zipArchive.Setup(x => x.GetEntry(It.IsAny<string>())).Returns(zipArchiveEntry.Object);
            zipArchiveEntry.Setup(x => x.OpenAsync()).ReturnsAsync(manifestStream);
            await JsonSerializer.SerializeAsync(manifestStream, manifest);

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
