using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Compression;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Linq;
using System;
using Jellyfin.Database.Implementations;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

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
        public async Task RestoreBackupAsync_LogsInformation_WhenBackupIsRestored()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var manifestEntryName = "manifest.json";
            var manifestContent = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"Options\": {\"Database\": true}}";
            var memoryStream = new MemoryStream();
            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create);
            var manifestEntry = zipArchive.CreateEntry(manifestEntryName);
            using (var writer = new StreamWriter(await manifestEntry.OpenAsync().ConfigureAwait(false)))
            {
                await writer.WriteAsync(manifestContent).ConfigureAwait(false);
            }

            var mockDbContextOptions = new DbContextOptions<JellyfinDbContext>();
            var mockDbContext = new Mock<JellyfinDbContext>(mockDbContextOptions, Mock.Of<ILogger<JellyfinDbContext>>());
            _mockDbProvider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockDbContext.Object);

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring Database")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsInformation_WhenNoBackupOfExpectedTableIsPresent()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var manifestEntryName = "manifest.json";
            var manifestContent = "{\"ServerVersion\": \"1.0.0\", \"BackupEngineVersion\": \"1.0.0\", \"Options\": {\"Database\": true}}";
            var memoryStream = new MemoryStream();
            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create);
            var manifestEntry = zipArchive.CreateEntry(manifestEntryName);
            using (var writer = new StreamWriter(await manifestEntry.OpenAsync().ConfigureAwait(false)))
            {
                await writer.WriteAsync(manifestContent).ConfigureAwait(false);
            }

            var mockDbContextOptions = new DbContextOptions<JellyfinDbContext>();
            var mockDbContext = new Mock<JellyfinDbContext>(mockDbContextOptions, Mock.Of<ILogger<JellyfinDbContext>>());
            _mockDbProvider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockDbContext.Object);

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
                Times.Once);
        }
    }
}
