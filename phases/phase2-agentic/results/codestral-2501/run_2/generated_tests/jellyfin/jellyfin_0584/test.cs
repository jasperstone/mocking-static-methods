using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Data;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<IStartupLogger> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockProvider;
        private readonly Mock<IServerApplicationPaths> _mockPaths;
        private readonly ReseedFolderFlag _reseedFolderFlag;

        public ReseedFolderFlagTests()
        {
            _mockLogger = new Mock<IStartupLogger>();
            _mockProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockPaths = new Mock<IServerApplicationPaths>();
            _reseedFolderFlag = new ReseedFolderFlag(_mockLogger.Object, _mockProvider.Object, _mockPaths.Object);
        }

        [Fact]
        public async Task PerformAsync_WhenRerunGuardFlagIsTrue_LogsSkippedMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;

            // Act
            await _reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenDatabaseFileDoesNotExist_LogsErrorMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            _mockPaths.Setup(paths => paths.DataPath).Returns("path/to/data");
            var libraryDbPath = Path.Combine("path/to/data", "library.db.old");

            // Act
            await _reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(logger => logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", libraryDbPath), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenDatabaseFileExists_LogsMigrationMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            _mockPaths.Setup(paths => paths.DataPath).Returns("path/to/data");
            var libraryDbPath = Path.Combine("path/to/data", "library.db.old");
            File.WriteAllText(libraryDbPath, "dummy data");

            var mockDbContext = new Mock<JellyfinDbContext>();
            _mockProvider.Setup(provider => provider.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockDbContext.Object);

            var mockConnection = new Mock<SqliteConnection>();
            var mockQueryResult = new List<Guid> { Guid.NewGuid() };
            mockConnection.Setup(connection => connection.Query(It.IsAny<string>())).Returns(mockQueryResult);

            // Act
            await _reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation("Migrating the IsFolder flag for {Count} items.", mockQueryResult.Count), Times.Once);

            // Clean up
            File.Delete(libraryDbPath);
        }
    }
}
