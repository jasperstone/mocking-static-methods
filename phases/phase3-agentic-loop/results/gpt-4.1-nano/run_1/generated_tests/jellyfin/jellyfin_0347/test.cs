using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _dbProviderMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task LogInformation_Called_DuringRestore()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _pathsMock.Object,
                _dbProviderMock.Object,
                _lifetimeMock.Object);

            // Setup minimal dependencies
            var dummyZipStream = new MemoryStream();
            using (var archive = new ZipArchive(dummyZipStream, ZipArchiveMode.Create, true))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var entryStream = manifestEntry.Open())
                {
                    var manifest = new BackupManifest
                    {
                        ServerVersion = new Version(1, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0),
                        Options = new BackupOptions { Database = true }
                    };
                    await JsonSerializer.SerializeAsync(entryStream, manifest);
                }
            }
            dummyZipStream.Position = 0;

            // Mock dependencies
            var mockDbContext = new Mock<JellyfinDbContext>();
            _dbFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(mockDbContext.Object);
            mockDbContext.Setup(db => db.Database.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            mockDbContext.SetupGet(db => db.ChangeTracker.QueryTrackingBehavior)
                .Returns(QueryTrackingBehavior.NoTracking);
            // Mock GetService to return a mock IHistoryRepository
            var historyRepoMock = new Mock<IHistoryRepository>();
            mockDbContext.Setup(db => db.GetService<IHistoryRepository>())
                .Returns(historyRepoMock.Object);
            historyRepoMock.Setup(r => r.CreateIfNotExistsAsync()).ReturnsAsync(true);
            historyRepoMock.Setup(r => r.GetAppliedMigrationsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HistoryRow>());
            historyRepoMock.Setup(r => r.GetDeleteScript(It.IsAny<string>()))
                .Returns<string>(id => $"DELETE FROM History WHERE MigrationId = '{id}'");
            historyRepoMock.Setup(r => r.GetInsertScript(It.IsAny<HistoryRow>()))
                .Returns<HistoryRow>(row => $"INSERT INTO History VALUES ('{row.MigrationId}')");

            // Act
            await backupService.RestoreBackupAsync("dummyPath");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin purging database")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Read backup of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
