using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class FixIncorrectOwnerIdRelationshipsTests
{
    [Fact]
    public async Task RemoveDuplicateItemsAsync_LogsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
        var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var persistenceServiceMock = new Mock<IItemPersistenceService>();

        var contextMock = new Mock<JellyfinDbContext>();
        dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

        var baseItems = new List<BaseItem>
        {
            new BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Folder", DateCreated = DateTime.Now },
            new BaseItem { Id = Guid.NewGuid(), Path = "path1", Type = "MediaBrowser.Controller.Entities.Video", DateCreated = DateTime.Now.AddDays(-1) },
            new BaseItem { Id = Guid.NewGuid(), Path = "path2", Type = "MediaBrowser.Controller.Entities.Folder", DateCreated = DateTime.Now }
        }.AsQueryable();

        var baseItemDbSetMock = new Mock<DbSet<BaseItem>>();
        baseItemDbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
        baseItemDbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
        baseItemDbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
        baseItemDbSetMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

        contextMock.Setup(c => c.BaseItems).Returns(baseItemDbSetMock.Object);

        var routine = new FixIncorrectOwnerIdRelationships(
            loggerMock.Object,
            dbContextFactoryMock.Object,
            libraryManagerMock.Object,
            persistenceServiceMock.Object);

        // Act
        await routine.PerformAsync(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()),
            Times.Once);
    }
}

public class BaseItem
{
    public Guid Id { get; set; }
    public string Path { get; set; }
    public string Type { get; set; }
    public DateTime DateCreated { get; set; }
    public Guid? OwnerId { get; set; }
    public int? ExtraType { get; set; }
    public Guid? ParentId { get; set; }
}
