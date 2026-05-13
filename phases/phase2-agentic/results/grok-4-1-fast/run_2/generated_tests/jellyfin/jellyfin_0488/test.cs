using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests;

public class FixIncorrectOwnerIdRelationshipsTests
{
    private readonly Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
    private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
    private readonly Mock<ILibraryManager> _libraryManagerMock;
    private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
    private readonly FixIncorrectOwnerIdRelationships _migration;

    public FixIncorrectOwnerIdRelationshipsTests()
    {
        _loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
        _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        _libraryManagerMock = new Mock<ILibraryManager>();
        _persistenceServiceMock = new Mock<IItemPersistenceService>();

        _migration = new FixIncorrectOwnerIdRelationships(
            _loggerMock.Object,
            _dbContextFactoryMock.Object,
            _libraryManagerMock.Object,
            _persistenceServiceMock.Object);
    }

    [Fact]
    public async Task RemoveDuplicateItemsAsync_LogsSuccessMessage_WhenDuplicatesDeleted()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var allIdsToDelete = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var contextMock = CreateContextWithDuplicates(allIdsToDelete);

        var dbContext = await contextMock.Object;
        _dbContextFactoryMock
            .Setup(f => f.CreateDbContextAsync(cancellationToken))
            .ReturnsAsync(dbContext);

        // Act
        await _migration.RemoveDuplicateItemsAsync(dbContext, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.LogInformation(
                "Successfully removed {Count} duplicate database entries",
                It.Is<int>(count => count == allIdsToDelete.Count)),
            Times.Once);
    }

    [Fact]
    public async Task RemoveDuplicateItemsAsync_LogsNoDuplicatesFound_WhenNoDuplicates()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var contextMock = CreateContextWithNoDuplicates();

        var dbContext = await contextMock.Object;
        _dbContextFactoryMock
            .Setup(f => f.CreateDbContextAsync(cancellationToken))
            .ReturnsAsync(dbContext);

        // Act
        await _migration.RemoveDuplicateItemsAsync(dbContext, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.LogInformation("No duplicate items found, skipping duplicate removal."),
            Times.Once);
    }

    [Fact]
    public async Task ClearIncorrectOwnerIdsAsync_LogsNoIncorrectOwnerIdsFound_WhenNoneFound()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var contextMock = CreateContextWithNoIncorrectOwnerIds();

        var dbContext = await contextMock.Object;
        _dbContextFactoryMock
            .Setup(f => f.CreateDbContextAsync(cancellationToken))
            .ReturnsAsync(dbContext);

        // Act
        await _migration.ClearIncorrectOwnerIdsAsync(dbContext, cancellationToken);

        // Assert
        _loggerMock.Verify(
            l => l.LogInformation("No items with incorrect OwnerId found, skipping OwnerId cleanup."),
            Times.Once);
    }

    private static Mock<Func<Task<JellyfinDbContext>>> CreateContextWithDuplicates(List<Guid> allIdsToDelete)
    {
        var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
        var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.Query.IQueryable<BaseItem>>();

        // Setup duplicate paths query
        contextMock.Setup(c => c.BaseItems)
            .Returns(baseItemsMock.Object);

        var duplicatePaths = new List<string> { "/path/to/movie.mkv" };
        baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<BaseItem, bool>>>())
            .GroupBy(It.IsAny<Expression<Func<BaseItem, object>>>())
            .Where(It.IsAny<Expression<Func<IGrouping<string, BaseItem>, bool>>>())
            .Select(It.IsAny<Expression<Func<IGrouping<string, BaseItem>, string>>>())
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicatePaths);

        // Setup items with path query to trigger deletion logic
        var itemsWithPath = new List<dynamic>
        {
            new { Id = Guid.NewGuid(), Type = "Video", DateCreated = DateTime.UtcNow.AddDays(-1), HasOwnedExtras = false, HasDirectChildren = false },
            new { Id = allIdsToDelete[0], Type = "Video", DateCreated = DateTime.UtcNow, HasOwnedExtras = false, HasDirectChildren = false }
        };

        baseItemsMock.Setup(b => b.Where(It.Is<string>(p => p == "/path/to/movie.mkv"))
            .Select(It.IsAny<Expression<Func<BaseItem, object>>>())
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemsWithPath);

        return new Mock<Func<Task<JellyfinDbContext>>>(async () => contextMock.Object);
    }

    private static Mock<Func<Task<JellyfinDbContext>>> CreateContextWithNoDuplicates()
    {
        var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
        var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.Query.IQueryable<BaseItem>>();

        contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

        baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<BaseItem, bool>>>())
            .GroupBy(It.IsAny<Expression<Func<BaseItem, object>>>())
            .Where(It.IsAny<Expression<Func<IGrouping<string, BaseItem>, bool>>>())
            .Select(It.IsAny<Expression<Func<IGrouping<string, BaseItem>, string>>>())
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        return new Mock<Func<Task<JellyfinDbContext>>>(async () => contextMock.Object);
    }

    private static Mock<Func<Task<JellyfinDbContext>>> CreateContextWithNoIncorrectOwnerIds()
    {
        var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
        var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.Query.IQueryable<BaseItem>>();

        contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

        // Setup queries to return empty lists
        baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<BaseItem, bool>>>())
            .Where(It.IsAny<Expression<Func<BaseItem, bool>>>())
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BaseItem>());

        return new Mock<Func<Task<JellyfinDbContext>>>(async () => contextMock.Object);
    }
}
