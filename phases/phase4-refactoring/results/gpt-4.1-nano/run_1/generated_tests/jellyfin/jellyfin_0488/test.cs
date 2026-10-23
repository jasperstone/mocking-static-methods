using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task LogInformation_Is_Called_When_PerformAsync_Is_Executed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var contextMock = new Mock<JellyfinDbContext>();
            var baseItems = new List<DbBaseItem>
            {
                new DbBaseItem { Id = Guid.NewGuid(), Path = "/path1" },
                new DbBaseItem { Id = Guid.NewGuid(), Path = "/path1" }
            }.AsQueryable();

            var dbSetMock = new Mock<DbSet<DbBaseItem>>();
            dbSetMock.As<IQueryable<DbBaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            dbSetMock.As<IQueryable<DbBaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            dbSetMock.As<IQueryable<DbBaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            dbSetMock.As<IQueryable<DbBaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(dbSetMock.Object);
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No duplicate items found, skipping duplicate removal.")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
