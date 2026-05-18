using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void Perform_LogsInformationMessage_WhenNoOrphanedAlternateVersionBaseItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                dbProviderMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
        }

        [Fact]
        public void Perform_LogsInformationMessage_WhenOrphanedAlternateVersionBaseItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                dbProviderMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Perform_LogsInformationMessage_WhenItemsFromDeletedLibrariesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                dbProviderMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} items from deleted libraries to remove.", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Perform_LogsInformationMessage_WhenNoItemsFromDeletedLibrariesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                dbProviderMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation("No items from deleted libraries found."), Times.Once);
        }
    }
}
