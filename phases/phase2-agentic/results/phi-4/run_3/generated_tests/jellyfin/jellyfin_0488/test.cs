using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsCorrectly_WhenDuplicatesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<DbSet<BaseItem>>();

            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            var duplicatePaths = new List<string> { "/path/to/duplicate1", "/path/to/duplicate2" };
            var duplicateItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate2" }
            };

            baseItemsMock.Setup(b => b.Where(It.IsAny<Func<BaseItem, bool>>()))
                .Returns((Func<BaseItem, bool> predicate) => duplicateItems.AsQueryable().Where(predicate));

            baseItemsMock.Setup(b => b.GroupBy(It.IsAny<Func<BaseItem, object>>()))
                .Returns((Func<BaseItem, object> keySelector) => duplicateItems.GroupBy(keySelector));

            var routine = new FixIncorrectOwnerIdRelationships(
                new StartupLogger<FixIncorrectOwnerIdRelationships>(loggerMock.Object),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            // Act
            await routine.RemoveDuplicateItemsAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully removed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
