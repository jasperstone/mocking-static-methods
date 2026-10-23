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

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_WhenRerunGuardFlagIsTrue_LogsSkipMessage()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            ReseedFolderFlag.RerunGuardFlag = true;

            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenLibraryDbDoesNotExist_LogsErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            pathsMock.Setup(p => p.DataPath).Returns("non_existent_path");
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenLibraryDbExists_LogsMigrationMessage()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            var dataPath = "existing_path";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            await File.WriteAllTextAsync(libraryDbPath, ""); // Use async file write

            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
            File.Delete(libraryDbPath); // Clean up the file
        }

        [Fact]
        public async Task PerformAsync_WhenQueryResultIsNotEmpty_LogsMigrationCountMessage()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            var dataPath = "existing_path";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            await File.WriteAllTextAsync(libraryDbPath, ""); // Use async file write

            var dbContextMock = new Mock<JellyfinDbContext>();
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<int>()), Times.Once);
            File.Delete(libraryDbPath); // Clean up the file
        }
    }
}
