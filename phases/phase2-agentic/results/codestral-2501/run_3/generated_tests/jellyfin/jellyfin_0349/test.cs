using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
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
        public async Task RestoreBackupAsync_LogsInformationWhenNoBackupOfExpectedTable()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var zipArchive = new ZipArchive(File.OpenRead(archivePath), ZipArchiveMode.Read, false);
            var dbContext = new JellyfinDbContext(new DbContextOptions<JellyfinDbContext>());

            _mockDbProvider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext);
            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("path/to/config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("path/to/data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("path/to/root");
            _mockApplicationPaths.Setup(x => x.InternalMetadataPath).Returns("path/to/internalmetadata");
            _mockApplicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("path/to/defaultinternalmetadata");

            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new BackupOptions { Database = true }
            };

            var manifestEntry = new Mock<ZipArchiveEntry>();
            manifestEntry.Setup(x => x.OpenAsync()).ReturnsAsync(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(manifest)));

            var zipArchiveMock = new Mock<ZipArchive>();
            zipArchiveMock.Setup(x => x.GetEntry(It.IsAny<string>())).Returns(manifestEntry.Object);

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No backup of expected table")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}
