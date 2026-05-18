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
    public class ReseedFolderFlagWrapper
    {
        private readonly ReseedFolderFlag _reseedFolderFlag;

        public ReseedFolderFlagWrapper(IStartupLogger logger, IDbContextFactory<JellyfinDbContext> provider, IServerApplicationPaths paths)
        {
            _reseedFolderFlag = new ReseedFolderFlag(logger, provider, paths);
        }

        public Task PerformAsync(CancellationToken cancellationToken)
        {
            return _reseedFolderFlag.PerformAsync(cancellationToken);
        }
    }

    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_ShouldLogInformation_WhenMigrationIsSkipped()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var cancellationToken = CancellationToken.None;

            ReseedFolderFlag.RerunGuardFlag = true;

            var reseedFolderFlag = new ReseedFolderFlagWrapper(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Migration is skipped because it does not apply."),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_ShouldLogInformation_WhenMigratingIsFolderFlag()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var cancellationToken = CancellationToken.None;

            pathsMock.Setup(x => x.DataPath).Returns("TestDataPath");
            providerMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);

            var reseedFolderFlag = new ReseedFolderFlagWrapper(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_ShouldLogError_WhenLibraryDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var cancellationToken = CancellationToken.None;

            pathsMock.Setup(x => x.DataPath).Returns("TestDataPath");

            var reseedFolderFlag = new ReseedFolderFlagWrapper(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_ShouldLogInformation_WhenMigratingIsFolderFlagForItems()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var cancellationToken = CancellationToken.None;

            pathsMock.Setup(x => x.DataPath).Returns("TestDataPath");
            providerMock.Setup(x => x.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);

            var reseedFolderFlag = new ReseedFolderFlagWrapper(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<int>()),
                Times.Once);
        }
    }
}
