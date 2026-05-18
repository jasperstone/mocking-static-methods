using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Tests.Migrations
{
    public class MigrateLinkedChildrenTests
    {
        private Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private Mock<ILibraryManager> _libraryManagerMock;
        private Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private Mock<JellyfinDbContext> _dbContextMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _dbContextMock = new Mock<JellyfinDbContext>();
            _dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(_dbContextMock.Object);
        }

        [Fact]
        public void LogInformation_Called_When_NoOrphanedBaseItems()
        {
            // Arrange
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            _dbContextMock.Setup(c => c.BaseItems).Returns(baseItems);
            _dbContextMock.Setup(c => c.LinkedChildren).Returns(linkedChildren);

            var routine = new MigrateLinkedChildren(_loggerMock.Object, _dbContextFactoryMock.Object, _libraryManagerMock.Object, null, null);

            // Act
            routine.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orphaned alternate version BaseItems found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_Called_When_OrphanedBaseItemsFound()
        {
            // Arrange
            var orphanedId = Guid.NewGuid();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = orphanedId, OwnerId = Guid.NewGuid(), ExtraType = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            _dbContextMock.Setup(c => c.BaseItems).Returns(baseItems);
            _dbContextMock.Setup(c => c.LinkedChildren).Returns(linkedChildren);

            var routine = new MigrateLinkedChildren(_loggerMock.Object, _dbContextFactoryMock.Object, _libraryManagerMock.Object, null, null);

            // Act
            routine.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 orphaned alternate version BaseItems to remove.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
