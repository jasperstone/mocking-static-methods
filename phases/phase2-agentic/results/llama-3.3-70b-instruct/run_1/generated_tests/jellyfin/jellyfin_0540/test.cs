using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Database;
using Jellyfin.Server.Media;
using Microsoft.Extensions.Logging;

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
        public void Perform_NoOrphanedVersionBaseItems_LogsMessageAndReturns()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            context.Setup(c => c.BaseItems).Returns(new List<BaseItem>().AsQueryable());
            _dbProviderMock.Setup(dp => dp.CreateDbContext()).Returns(context.Object);
            var migrateLinkedChildren = new MigrateLinkedChildren(_loggerMock.Object, _dbProviderMock.Object, _libraryManagerMock.Object, _appHostMock.Object, _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
        }

        [Fact]
        public void Perform_OrphanedVersionBaseItems_LogsMessagesAndDeletesItems()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() }
            };
            context.Setup(c => c.BaseItems).Returns(baseItems.AsQueryable());
            _dbProviderMock.Setup(dp => dp.CreateDbContext()).Returns(context.Object);
            _libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(new BaseItem());
            var migrateLinkedChildren = new MigrateLinkedChildren(_loggerMock.Object, _dbProviderMock.Object, _libraryManagerMock.Object, _appHostMock.Object, _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 2), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Removed {Count} orphaned alternate version BaseItems.", 2), Times.Once);
            _libraryManagerMock.Verify(lm => lm.DeleteItemsUnsafeFast(It.IsAny<List<BaseItem>>()), Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoItems_LogsMessageAndReturns()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            context.Setup(c => c.BaseItems).Returns(new List<BaseItem>().AsQueryable());
            _dbProviderMock.Setup(dp => dp.CreateDbContext()).Returns(context.Object);
            var migrateLinkedChildren = new MigrateLinkedChildren(_loggerMock.Object, _dbProviderMock.Object, _libraryManagerMock.Object, _appHostMock.Object, _appPathsMock.Object);

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(context.Object);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("No items from deleted libraries found."), Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_Items_LogsMessagesAndDeletesItems()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            };
            context.Setup(c => c.BaseItems).Returns(baseItems.AsQueryable());
            _dbProviderMock.Setup(dp => dp.CreateDbContext()).Returns(context.Object);
            _libraryManagerMock.Setup(lm => lm.GetItemById(It.IsAny<Guid>())).Returns(new BaseItem());
            var migrateLinkedChildren = new MigrateLinkedChildren(_loggerMock.Object, _dbProviderMock.Object, _libraryManagerMock.Object, _appHostMock.Object, _appPathsMock.Object);

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(context.Object);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Found {Count} items from deleted libraries to remove.", 2), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Removed {Count} items from deleted libraries.", 2), Times.Once);
            _libraryManagerMock.Verify(lm => lm.DeleteItemsUnsafeFast(It.IsAny<List<BaseItem>>()), Times.Once);
        }
    }
}
