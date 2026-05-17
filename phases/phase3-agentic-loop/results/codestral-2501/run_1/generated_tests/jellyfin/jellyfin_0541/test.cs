using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller;
using Jellyfin.Database.Implementations.Entities;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid(), Type = "SomeType" }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<BaseItemEntity>>();
            dbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            dbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            dbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            dbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            dbContextMock.Setup(c => c.BaseItems).Returns(dbSetMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>());

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
