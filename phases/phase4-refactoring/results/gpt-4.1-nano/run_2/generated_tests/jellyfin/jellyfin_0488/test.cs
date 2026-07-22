using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsInformation_WhenDuplicatesExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItems = new List<Database.BaseItem>
            {
                new Database.BaseItem { Id = Guid.NewGuid(), Path = "/path/to/file1" },
                new Database.BaseItem { Id = Guid.NewGuid(), Path = "/path/to/file1" },
                new Database.BaseItem { Id = Guid.NewGuid(), Path = "/path/to/file2" }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<Database.BaseItem>>();
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            dbSetMock.As<IQueryable<Database.BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            var context = new Mock<JellyfinDbContext>();
            context.Setup(c => c.BaseItems).Returns(dbSetMock.Object);
            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                new Mock<IDbContextFactory<JellyfinDbContext>>().Object,
                new Mock<ILibraryManager>().Object,
                new Mock<IItemPersistenceService>().Object);
            // Inject context mock
            // Note: For simplicity, assuming RemoveDuplicateItemsAsync is made public or internal for testing
            // and that it accepts the context as parameter.

            // Act
            await routine.RemoveDuplicateItemsAsync(context.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found") && v.ToString().Contains("paths with duplicate database entries")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
