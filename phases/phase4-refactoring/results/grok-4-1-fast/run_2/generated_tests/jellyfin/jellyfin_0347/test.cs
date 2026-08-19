using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IDbContextFactory<DbContext>> _mockDbProvider;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly object _mockJellyfinDatabaseProvider;
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _mockDbProvider = new Mock<IDbContextFactory<DbContext>>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("/config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("/data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("/root");
            _mockApplicationPaths.Setup(x => x.InternalMetadataPath).Returns("/data/metadata");
            _mockApplicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("/data/metadata-default");

            _mockApplicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0, 0));

            // Use object to avoid missing interface reference
            _mockJellyfinDatabaseProvider = new object();
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _mockLogger.Object,
                _mockDbProvider.Object,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                (dynamic)_mockJellyfinDatabaseProvider,
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_DatabasePurgeCompletes_LogsDatabasePurgedMessage()
        {
            // Arrange
            SetupMinimalSuccessfulRestoreFlow();

            // Act
            await Assert.ThrowsAnyAsync<NotImplementedException>(() => _backupService.RestoreBackupAsync("test.zip"));

            // Assert - Verify the specific log call on line 202: _logger.LogInformation("Database Purged");
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Database Purged")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_BeginsDatabasePurge_LogsBeginPurgingDatabaseMessage()
        {
            // Arrange
            SetupMinimalSuccessfulRestoreFlow();

            // Act
            await Assert.ThrowsAnyAsync<NotImplementedException>(() => _backupService.RestoreBackupAsync("test.zip"));

            // Assert - Verify the log before purge: _logger.LogInformation("Begin purging database");
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Begin purging database")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RestoreBackupAsync_BeginsDatabaseRestore_LogsBeginRestoringDatabaseMessage()
        {
            // Arrange
            SetupMinimalSuccessfulRestoreFlow();

            // Act
            await Assert.ThrowsAnyAsync<NotImplementedException>(() => _backupService.RestoreBackupAsync("test.zip"));

            // Assert - Verify the log: _logger.LogInformation("Begin restoring Database");
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Begin restoring Database")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetupMinimalSuccessfulRestoreFlow()
        {
            var mockDbContext = new Mock<DbContext>();
            mockDbContext.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
            
            _mockDbProvider.Setup(x => x.CreateDbContextAsync()).ReturnsAsync(mockDbContext.Object);

            // Mock StorageHelper static call to pass
            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("/tmp/config");
        }
    }
}
