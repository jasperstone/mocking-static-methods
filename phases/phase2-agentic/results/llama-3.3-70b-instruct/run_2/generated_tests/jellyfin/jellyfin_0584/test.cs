using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformationMessage_WhenMigratingIsFolderFlag()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task PerformAsync_LogsInformationMessage_WithCorrectCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.SetupGet(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);
            var connectionMock = new Mock<SqliteConnection>();
            connectionMock.Setup(c => c.Query(It.IsAny<string>())).Returns(new[] { new { GetGuid = (Func<int, Guid>)((int index) => Guid.NewGuid()) } });
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Migrating the IsFolder flag for {Count} items.", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsError_WhenLibraryDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.SetupGet(p => p.DataPath).Returns("DataPath");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
