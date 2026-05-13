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
                loggerMock.Object,
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>());

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(Mock.Of<JellyfinDbContext>());

            // Assert
            loggerMock.Verify(l => l.LogInformation("No items from deleted libraries found."), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenOrphanedItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                loggerMock.Object,
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>());

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(Mock.Of<JellyfinDbContext>());

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} items from deleted libraries to remove.", It.IsAny<int>()), Times.Once);
        }
    }
}
