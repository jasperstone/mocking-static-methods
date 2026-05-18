using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database;
using MediaBrowser.Common.Application;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void Perform_LogInformationCalled_WhenNoOrphanedAlternateVersionBaseItemsFound()
        {
            // Arrange
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
        }

        [Fact]
        public void Perform_LogInformationCalled_WhenOrphanedAlternateVersionBaseItemsFound()
        {
            // Arrange
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Perform_LogInformationCalled_WhenItemsFromDeletedLibrariesFound()
        {
            // Arrange
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Found {Count} items from deleted libraries to remove.", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Perform_LogInformationCalled_WhenNoItemsFromDeletedLibrariesFound()
        {
            // Arrange
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("No items from deleted libraries found."), Times.Once);
        }
    }
}
