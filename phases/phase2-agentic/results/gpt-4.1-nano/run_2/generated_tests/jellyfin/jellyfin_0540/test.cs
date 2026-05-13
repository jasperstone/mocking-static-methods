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
        [Fact]
        public void LogInformation_Called_When_NoOrphanedBaseItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null, Path = "path1", Type = "type1", Data = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>();

            var dbSetBaseItemsMock = new Mock<DbSet<BaseItem>>();
            dbSetBaseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            dbSetBaseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            dbSetBaseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            dbSetBaseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            var linkedChildrenMock = new Mock<DbSet<LinkedChildEntity>>();
            linkedChildrenMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.Provider).Returns(linkedChildren.AsQueryable().Provider);
            linkedChildrenMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.Expression).Returns(linkedChildren.AsQueryable().Expression);
            linkedChildrenMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.ElementType).Returns(linkedChildren.AsQueryable().ElementType);
            linkedChildrenMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.GetEnumerator()).Returns(linkedChildren.AsQueryable().GetEnumerator());

            dbContextMock.Setup(c => c.BaseItems).Returns(dbSetBaseItemsMock.Object);
            dbContextMock.Setup(c => c.LinkedChildren).Returns(linkedChildrenMock.Object);

            var routine = new MigrateLinkedChildren(
                new Mock<ILoggerFactory>().Object,
                new Mock<IDbContextFactory<JellyfinDbContext>>().Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);
            // Inject the logger mock
            var routineWithLogger = new MigrateLinkedChildren(
                new Mock<ILoggerFactory>().Object,
                new Mock<IDbContextFactory<JellyfinDbContext>>().Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);
            // Use reflection to set the private _logger field
            var loggerField = typeof(MigrateLinkedChildren).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(routineWithLogger, loggerMock.Object);

            // Act
            routineWithLogger.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orphaned alternate version BaseItems found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
