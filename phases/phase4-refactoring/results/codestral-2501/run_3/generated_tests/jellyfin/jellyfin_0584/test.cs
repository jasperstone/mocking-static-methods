using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_ShouldSkipMigration_WhenRerunGuardFlagIsTrue()
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
        public async Task PerformAsync_ShouldLogError_WhenLibraryDbOldDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            pathsMock.Setup(p => p.DataPath).Returns("/nonexistent/path");
            ReseedFolderFlag.RerunGuardFlag = false;

            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_ShouldLogInformation_WhenMigratingItems()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var dbContextMock = new Mock<JellyfinDbContext>();

            pathsMock.Setup(p => p.DataPath).Returns("/valid/path");
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);
            ReseedFolderFlag.RerunGuardFlag = false;

            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<object[]>()), Times.Once);
        }
    }
}
