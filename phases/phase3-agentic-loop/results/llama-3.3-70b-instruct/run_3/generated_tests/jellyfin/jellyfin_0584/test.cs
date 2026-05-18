using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
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
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, pathsMock.Object, providerMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Never);
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, pathsMock.Object, providerMock.Object);
            ReseedFolderFlag.RerunGuardFlag = true;

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Never);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenLibraryDbExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, pathsMock.Object, providerMock.Object);

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
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, pathsMock.Object, providerMock.Object);

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
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, pathsMock.Object, providerMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", 0), Times.Once);
        }
    }
}
