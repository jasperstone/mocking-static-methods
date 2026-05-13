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

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<IStartupLogger> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbContextFactory;
        private readonly Mock<IServerApplicationPaths> _mockServerApplicationPaths;
        private readonly ReseedFolderFlag _reseedFolderFlag;

        public ReseedFolderFlagTests()
        {
            _mockLogger = new Mock<IStartupLogger>();
            _mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockServerApplicationPaths = new Mock<IServerApplicationPaths>();

            _reseedFolderFlag = new ReseedFolderFlag(
                _mockLogger.Object,
                _mockDbContextFactory.Object,
                _mockServerApplicationPaths.Object);
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
            _mockServerApplicationPaths.Setup(paths => paths.DataPath).Returns("TestDataPath");
            var libraryDbPath = Path.Combine("TestDataPath", "library.db.old");

            // Act
            await _reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(logger => logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", libraryDbPath), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenDatabaseFileExists_MigratesIsFolderFlag()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            _mockServerApplicationPaths.Setup(paths => paths.DataPath).Returns("TestDataPath");
            var libraryDbPath = Path.Combine("TestDataPath", "library.db.old");
            File.WriteAllText(libraryDbPath, string.Empty); // Create an empty file to simulate the existence of the database file

            var mockDbContext = new Mock<JellyfinDbContext>();
            _mockDbContextFactory.Setup(factory => factory.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockDbContext.Object);

            var mockBaseItems = new Mock<DbSet<BaseItem>>();
            mockDbContext.Setup(context => context.BaseItems).Returns(mockBaseItems.Object);

            var queryResult = new List<Guid> { Guid.NewGuid() };
            mockBaseItems.Setup(items => items.Where(It.IsAny<Func<BaseItem, bool>>())).Returns(mockBaseItems.Object);
            mockBaseItems.Setup(items => items.ExecuteUpdateAsync(It.IsAny<Func<BaseItem, BaseItem>>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            await _reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation("Migrating the IsFolder flag for {Count} items.", queryResult.Count), Times.Once);
            mockBaseItems.Verify(items => items.ExecuteUpdateAsync(It.IsAny<Func<BaseItem, BaseItem>>(), It.IsAny<CancellationToken>()), Times.Once);

            // Clean up
            File.Delete(libraryDbPath);
        }
    }
}
