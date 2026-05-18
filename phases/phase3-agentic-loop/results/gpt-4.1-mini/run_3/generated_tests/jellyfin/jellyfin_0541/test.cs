using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Migrations.Routines
{
    internal class MigrateLinkedChildrenTests
    {
        [Fact]
        public void Perform_LogsNoItemsFromDeletedLibrariesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            // Setup BaseItems DbSet to simulate no orphaned items for CleanupItemsFromDeletedLibraries
            var baseItemsData = new List<BaseItemEntity>
            {
                // Items with TopParentId that all exist in BaseItems (no orphaned)
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid(), Type = "MediaBrowser.Controller.Entities.Video", Data = "{}", Path = null },
                new BaseItemEntity { Id = Guid.NewGuid(), TopParentId = null, Type = "MediaBrowser.Controller.Entities.Movies.Movie", Data = "{}", Path = null }
            }.AsQueryable();

            var baseItemsDbSetMock = CreateDbSetMock(baseItemsData);

            var linkedChildrenDbSetMock = CreateDbSetMock(new List<LinkedChildEntity>().AsQueryable());

            var dbContextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
            dbContextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSetMock.Object);
            dbContextMock.Setup(c => c.LinkedChildren).Returns(linkedChildrenDbSetMock.Object);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();

            var migration = new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            migration.Perform();

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

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
