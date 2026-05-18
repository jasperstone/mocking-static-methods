using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformation_WhenRerunGuardFlagIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Never);
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenLibraryDbExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsError_WhenLibraryDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", "DataPath/library.db.old"), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenMigratingIsFolderFlag()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<int>()), Times.Once);
        }
    }
}
