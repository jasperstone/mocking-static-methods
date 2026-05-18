using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Entities;
using Jellyfin.Extensions;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
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
        public void Perform_NoOrphanedVersionBaseItemsFound_LogsMessage()
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
        public void Perform_OrphanedVersionBaseItemsFound_LogsMessageAndDeletesItems()
        {
            // Arrange
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() }
            };
            var context = new Mock<JellyfinDbContext>();
            context.Setup(c => c.BaseItems).Returns(baseItems.AsQueryable());
            _dbProviderMock.Setup(dp => dp.CreateDbContext()).Returns(context.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(_loggerMock.Object, _dbProviderMock.Object, _libraryManagerMock.Object, _appHostMock.Object, _appPathsMock.Object);

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", baseItems.Count), Times.Once);
            _libraryManagerMock.Verify(lm => lm.DeleteItemsUnsafeFast(It.IsAny<List<BaseItem>>()), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Removed {Count} orphaned alternate version BaseItems.", baseItems.Count), Times.Once);
        }
    }
}
