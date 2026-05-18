using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                null,
                null,
                null,
                null);

            // Act
            migrateLinkedChildren._logger = loggerMock.Object;
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Starting cleanup of items from deleted libraries..."), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenOrphanedVersionIdsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                null,
                null,
                null,
                null);
            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(new[] { new BaseItem { Id = 1, OwnerId = 1, ExtraType = null } }.AsQueryable());

            // Act
            migrateLinkedChildren._logger = loggerMock.Object;
            migrateLinkedChildren.CleanupOrphanedAlternateVersions(contextMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 1), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenNoOrphanedVersionIdsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                null,
                null,
                null,
                null);
            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(new BaseItem[0].AsQueryable());

            // Act
            migrateLinkedChildren._logger = loggerMock.Object;
            migrateLinkedChildren.CleanupOrphanedAlternateVersions(contextMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
        }
    }
}
