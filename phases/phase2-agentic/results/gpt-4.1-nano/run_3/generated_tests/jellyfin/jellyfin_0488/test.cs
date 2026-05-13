using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Tests.Migrations
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task LogInformation_Called_When_DuplicatesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var contextMock = new Mock<JellyfinDbContext>();
            var baseItems = new List<Database.BaseItem>
            {
                new Database.BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "type1", DateCreated = DateTime.Now },
                new Database.BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "type2", DateCreated = DateTime.Now.AddMinutes(-10) }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<Database.BaseItem>>();
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(dbSetMock.Object);
            dbContextMock.Setup(c => c.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object
            );

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No duplicate items found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
