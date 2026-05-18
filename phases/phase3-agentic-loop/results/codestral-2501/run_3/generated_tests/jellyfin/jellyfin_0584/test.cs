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
        [Fact]
        public async Task PerformAsync_ShouldLogSkippedMessage_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            var reseedFolderFlag = new TestableReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);
            reseedFolderFlag.RerunGuardFlag = true;

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_ShouldLogError_WhenLibraryDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            pathsMock.Setup(p => p.DataPath).Returns("non_existent_path");

            var reseedFolderFlag = new TestableReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_ShouldLogInformation_WhenMigratingItems()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<DbSet<BaseItem>>();

            pathsMock.Setup(p => p.DataPath).Returns("valid_path");
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);
            dbContextMock.Setup(d => d.BaseItems).Returns(baseItemsMock.Object);

            var reseedFolderFlag = new TestableReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<int>()), Times.Once);
        }

        private class TestableReseedFolderFlag : ReseedFolderFlag
        {
            public TestableReseedFolderFlag(IStartupLogger logger, IDbContextFactory<JellyfinDbContext> provider, IServerApplicationPaths paths)
                : base(logger, provider, paths)
            {
            }

            public new static bool RerunGuardFlag
            {
                get => ReseedFolderFlag.RerunGuardFlag;
                set => ReseedFolderFlag.RerunGuardFlag = value;
            }
        }
    }
}
