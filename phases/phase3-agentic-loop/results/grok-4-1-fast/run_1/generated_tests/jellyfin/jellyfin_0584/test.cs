using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<ILogger<ReseedFolderFlag>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _providerMock;

        public ReseedFolderFlagTests()
        {
            _loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        }

        [Fact]
        public async Task PerformAsync_WhenRerunGuardFlagIsTrue_LogsSkipMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var target = CreateTarget();

            // Act
            await target.PerformAsync(default);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Migration is skipped because it does not apply."), 
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenLibraryDbDoesNotExist_LogsErrorMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var dataPath = "/fake/data";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            _pathsMock.Setup(x => x.DataPath).Returns(dataPath);
            var target = CreateTarget();

            // Act
            await target.PerformAsync(default);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    "Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.",
                    libraryDbPath),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_WhenQueryReturnsItems_LogsCountMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var dataPath = "/fake/data";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            _pathsMock.Setup(x => x.DataPath).Returns(libraryDbPath);

            // Mock File.Exists to return true
            var originalExists = File.Exists;
            File.Exists = (path) => path == libraryDbPath;

            // Mock DbContext creation
            var dbContextMock = new Mock<JellyfinDbContext>();
            _providerMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            var target = CreateTarget();

            try
            {
                // Act
                await target.PerformAsync(default);
            }
            finally
            {
                File.Exists = originalExists;
            }

            // Assert - Verifies the LogInformation call at line 67 is executed
            // Even if query returns 0 items, the log call is hit
            _loggerMock.Verify(
                x => x.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<int[]>()),
                Times.Once);
        }

        private ReseedFolderFlag CreateTarget() =>
            new ReseedFolderFlag(_loggerMock.Object, _providerMock.Object, _pathsMock.Object);
    }
}
