using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations.Entities;
using System;

namespace Jellyfin.Server.Migrations.Routines.Tests
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

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<BaseItem>>();
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            dbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            dbContextMock.Setup(c => c.BaseItems).Returns(dbSetMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

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
