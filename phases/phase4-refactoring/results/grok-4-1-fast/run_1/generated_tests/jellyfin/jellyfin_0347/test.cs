using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
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
        public async Task RestoreBackupAsync_DatabasePurged_LogsDatabasePurgedMessage()
        {
            // Arrange
            var archivePath = "test-backup.zip";
            var zipContent = CreateMinimalBackupZip();
            await File.WriteAllBytesAsync(archivePath, zipContent);

            var dbContextMock = new Mock<JellyfinDbContext>();
            var databaseMock = new Mock<DatabaseFacade>(MockBehavior.Strict, dbContextMock.Object);
            databaseMock.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);
            
            var historyRepoMock = new Mock<IHistoryRepository>();
            historyRepoMock.Setup(h => h.CreateIfNotExistsAsync()).Returns(Task.CompletedTask);
            historyRepoMock.Setup(h => h.GetAppliedMigrationsAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new List<object>());
            
            dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            dbContextMock.Setup(c => c.ChangeTracker).Returns(new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker>().Object);
            dbContextMock.Setup(c => c.Model).Returns(new Mock<IModel>().Object);
            dbContextMock.Setup(c => c.GetService<IHistoryRepository>()).Returns(historyRepoMock.Object);

            _applicationHostMock.Setup(h => h.ApplicationVersion).Returns(new Version(10, 8, 0, 0));

            _dbProviderMock.Setup(p => p.CreateDbContextAsync())
                .ReturnsAsync(dbContextMock.Object);

            // Mock the PurgeDatabase call to complete successfully
            _jellyfinDatabaseProviderMock.Setup(p => p.PurgeDatabase(
                It.IsAny<JellyfinDbContext>(),
                It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask);

            // Setup reflection to return no entity types to avoid further processing
            dbContextMock.Setup(c => c.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .Returns(Array.Empty<PropertyInfo>());

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert - verify the specific LogInformation call on line 202
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v!.ToString()!.Contains("Database Purged")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            File.Delete(archivePath);
        }

        private byte[] CreateMinimalBackupZip()
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using var manifestStream = manifestEntry.Open();
                var manifestJson = JsonSerializer.Serialize(new BackupManifest
                {
                    ServerVersion = new Version(10, 8, 0),
                    BackupEngineVersion = new Version(0, 2, 0),
                    Options = new BackupOptions { Database = true }
                });
                using var writer = new StreamWriter(manifestStream);
                writer.Write(manifestJson);
            }
            return memoryStream.ToArray();
        }
    }

    // Minimal classes for compilation
    public class BackupManifest
    {
        public Version ServerVersion { get; set; } = null!;
        public Version BackupEngineVersion { get; set; } = null!;
        public BackupOptions Options { get; set; } = null!;
    }

    public class BackupOptions
    {
        public bool Database { get; set; }
    }
}
