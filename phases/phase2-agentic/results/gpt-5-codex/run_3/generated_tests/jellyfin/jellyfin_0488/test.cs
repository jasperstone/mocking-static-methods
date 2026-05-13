using System;
using System.Collections.Generic;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines;

public class FixIncorrectOwnerIdRelationshipsTests
{
    [Fact]
    public void PerformAsync_RemovesDuplicates_LogsInformation()
    {
        // Arrange
        var duplicatePathId1 = Guid.NewGuid();
        var duplicatePathId2 = Guid.NewGuid();
        var expectedDuplicateCount = 1;

        var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>(MockBehavior.Strict);
        loggerMock.Setup(l => l.LogInformation("No duplicate items found, skipping duplicate removal."));

        var dbContextFactoryMock = TestHelpers.CreateDbContextFactoryWithDuplicateEntries(
            duplicatePathId1,
            duplicatePathId2);

        var libraryManagerMock = new Mock<ILibraryManager>();
        libraryManagerMock.Setup(l => l.GetItemById(duplicatePathId1)).Returns((IHasId?)null);
        libraryManagerMock.Setup(l => l.GetItemById(duplicatePathId2)).Returns((IHasId?)null);

        var persistenceServiceMock = new Mock<IItemPersistenceService>();

        var migration = new FixIncorrectOwnerIdRelationships(
            loggerMock.Object,
            dbContextFactoryMock.Object,
            libraryManagerMock.Object,
            persistenceServiceMock.Object);

        // Act
        migration.PerformAsync(default).GetAwaiter().GetResult();

        // Assert
        loggerMock.Verify(l =>
            l.LogInformation("Successfully removed {Count} duplicate database entries", expectedDuplicateCount),
            Times.Once);
    }
}

internal static class TestHelpers
{
    public static Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>
        CreateDbContextFactoryWithDuplicateEntries(Guid duplicatePathId1, Guid duplicatePathId2)
    {
        var mockFactory = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
        var mockContext = new Mock<Jellyfin.Database.Implementations.JellyfinDbContext>();

        mockContext
            .Setup(c => c.BaseItems)
            .Returns(DbSetMock.Create(new List<Jellyfin.Database.Implementations.BaseItem>
            {
                new Jellyfin.Database.Implementations.BaseItem { Id = duplicatePathId1, Path = "duplicatePath" },
                new Jellyfin.Database.Implementations.BaseItem { Id = duplicatePathId2, Path = "duplicatePath" }
            }));

        mockFactory
            .Setup(f => f.CreateDbContextAsync(default))
            .ReturnsAsync(mockContext.Object);

        return mockFactory;
    }
}

internal static class DbSetMock
{
    public static Microsoft.EntityFrameworkCore.DbSet<T> Create<T>(IEnumerable<T> entities) where T : class
    {
        var queryable = entities.AsQueryable();

        var dbSetMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return dbSetMock.Object;
    }
}
