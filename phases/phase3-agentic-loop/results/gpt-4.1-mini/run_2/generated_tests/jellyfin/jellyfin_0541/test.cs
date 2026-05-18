using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void Perform_LogsNoItemsFromDeletedLibrariesFound_WhenNoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            var baseItemsData = new List<BaseItemEntity>
            {
                // No items with TopParentId pointing to missing library
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid(), Type = "MediaBrowser.Controller.Entities.Video", Data = null, Path = null }
            }.AsQueryable();

            var baseItemsDbSetMock = new Mock<DbSet<BaseItemEntity>>();
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(baseItemsData.Provider);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(baseItemsData.Expression);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(baseItemsData.ElementType);
            baseItemsDbSetMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(baseItemsData.GetEnumerator());

            var linkedChildrenDbSetMock = new Mock<DbSet<LinkedChildEntity>>();
            linkedChildrenDbSetMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.Provider).Returns(Enumerable.Empty<LinkedChildEntity>().AsQueryable().Provider);
            linkedChildrenDbSetMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.Expression).Returns(Enumerable.Empty<LinkedChildEntity>().AsQueryable().Expression);
            linkedChildrenDbSetMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<LinkedChildEntity>().AsQueryable().ElementType);
            linkedChildrenDbSetMock.As<IQueryable<LinkedChildEntity>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<LinkedChildEntity>().GetEnumerator());

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSetMock.Object);
            dbContextMock.Setup(c => c.LinkedChildren).Returns(linkedChildrenDbSetMock.Object);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var libraryManagerMock = new Mock<ILibraryManager>();

            // For appHost and appPaths, pass null since they are not used in this test
            var migrate = new MigrateLinkedChildren(loggerFactoryMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, null, null);

            // Act
            migrate.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
