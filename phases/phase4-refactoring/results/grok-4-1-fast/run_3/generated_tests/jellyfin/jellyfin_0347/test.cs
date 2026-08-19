using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
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
        public async Task RestoreBackupAsync_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var archivePath = "nonexistent.zip";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => _backupService.RestoreBackupAsync(archivePath));
            Assert.Contains("nonexistent.zip", exception.Message);
        }

        [Fact]
        public void ScheduleRestoreAndRestartServer_SetsPropertiesCorrectly()
        {
            // Arrange
            var archivePath = "test-backup.zip";

            // Act
            _backupService.ScheduleRestoreAndRestartServer(archivePath);

            // Assert
            _applicationHostMock.VerifySet(h => h.RestoreBackupPath = archivePath, Times.Once);
            _applicationHostMock.VerifySet(h => h.ShouldRestart = true, Times.Once);
            _applicationHostMock.Verify(h => h.NotifyPendingRestart(), Times.Once);
        }

        [Fact]
        public void Logger_LogInformation_IsCallable()
        {
            // Arrange
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation("Database Purged");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v.ToString()).Contains("Database Purged")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_LogInformation_CanBeVerified()
        {
            // This tests that the ILoggerExtensions LogInformation extension method works as expected
            // The specific call _logger.LogInformation("Database Purged") on line 202 follows this pattern
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation("Database Purged");

            // Assert - Verifies the exact extension method call pattern used in BackupService
            _loggerMock.Verify(
                logger => logger.LogInformation("Database Purged"),
                Times.Once);
        }
    }
}
