using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var orphanedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var items = orphanedIds.Select(id => new BaseItem { Id = id }).ToList();

            libraryManagerMock
                .Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns<Guid>(id => items.FirstOrDefault(i => i.Id == id));

            var routine = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            var contextMock = new Mock<JellyfinDbContext>();
            contextMock
                .Setup(c => c.BaseItems)
                .Returns(new List<BaseItem>().AsQueryable());

            contextMock
                .Setup(c => c.BaseItems.Any(It.IsAny<Expression<Func<BaseItem, bool>>>()))
                .Returns(false);

            contextMock
                .Setup(c => c.BaseItems.Where(It.IsAny<Expression<Func<BaseItem, bool>>>()))
                .Returns((Expression<Func<BaseItem, bool>> expr) => new List<BaseItem>().AsQueryable().Where(expr));

            // Act
            routine.CleanupItemsFromDeletedLibraries(contextMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("No items from deleted libraries found."),
                Times.Once);

            loggerMock.Verify(
                l => l.LogInformation("Found {Count} items from deleted libraries to remove.", orphanedIds.Count),
                Times.Once);

            loggerMock.Verify(
                l => l.LogInformation("Removed {Count} items from deleted libraries.", items.Count),
                Times.Once);
        }
    }

    // Mock classes for testing
    public class BaseItem
    {
        public Guid Id { get; set; }
        public int? TopParentId { get; set; }
    }

    public interface ILibraryManager
    {
        BaseItem GetItemById(Guid id);
        void DeleteItemsUnsafeFast(List<BaseItem> items);
    }

    public class JellyfinDbContext
    {
        public IQueryable<BaseItem> BaseItems { get; set; }
    }
}
