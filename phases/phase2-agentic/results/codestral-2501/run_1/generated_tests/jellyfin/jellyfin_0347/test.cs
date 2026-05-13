using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
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
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var archivePath = "path/to/archive.zip";
            var dbContextMock = new Mock<JellyfinDbContext>();
            _dbProviderMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            var zipArchiveMock = new Mock<ZipArchive>(Stream.Null, ZipArchiveMode.Read);
            var zipArchiveEntryMock = new Mock<ZipArchiveEntry>();
            zipArchiveMock.Setup(x => x.GetEntry(It.IsAny<string>())).Returns(zipArchiveEntryMock.Object);

            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                DateCreated = DateTimeOffset.Now,
                DatabaseTables = new string[] { },
                Options = new BackupOptions { Database = true }
            };

            var manifestStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(manifestStream, manifest);
            manifestStream.Position = 0;
            zipArchiveEntryMock.Setup(x => x.OpenAsync()).ReturnsAsync(manifestStream);

            _applicationHostMock.Setup(x => x.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
