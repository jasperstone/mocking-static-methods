using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MigrateLinkedChildrenTests
{
    [Fact]
    public void CleanupItemsFromDeletedLibraries_NoItemsFound_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var contextMock = new Mock<JellyfinDbContext>();

        // Simulate no orphaned items
        contextMock.Setup(c => c.BaseItems)
            .ReturnsDbSet(new List<BaseItem>());

        var routine = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>())
        {
            _logger = loggerMock.Object,
            _libraryManager = libraryManagerMock.Object,
            _dbProvider = Mock.Of<IDbContextFactory<JellyfinDbContext>>(f => f.CreateDbContext() == contextMock.Object)
        };

        // Act
        routine.CleanupItemsFromDeletedLibraries(contextMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("No items from deleted libraries found."),
            Times.Once);
    }

    [Fact]
    public void CleanupItemsFromDeletedLibraries_NoItemsFound_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var contextMock = new Mock<JellyfinDbContext>();

        // Simulate no orphaned items
        contextMock.Setup(c => c.BaseItems)
            .ReturnsDbSet(new List<BaseItem>());

        var routine = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>())
        {
            _logger = loggerMock.Object,
            _libraryManager = libraryManagerMock.Object,
            _dbProvider = Mock.Of<IDbContextFactory<JellyfinDbContext>>(f => f.CreateDbContext() == contextMock.Object)
        };

        // Act
        routine.CleanupItemsFromDeletedLibraries(contextMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("No items from deleted libraries found."),
            Times.Once);
    }

    [Fact]
    public void CleanupItemsFromDeletedLibraries_ItemsFound_LogsCorrectMessages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var contextMock = new Mock<JellyfinDbContext>();

        var orphanedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var baseItems = new List<BaseItem>
        {
            new BaseItem { Id = orphanedIds[0], TopParentId = Guid.NewGuid() },
            new BaseItem { Id = orphanedIds[1], TopParentId = Guid.NewGuid() }
        };
        contextMock.Setup(c => c.BaseItems)
            .ReturnsDbSet(baseItems);

        libraryManagerMock
            .Setup(m => m.GetItemById(It.IsAny<Guid>()))
            .Returns((Guid id) => baseItems.FirstOrDefault(b => b.Id == id));

        var routine = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>())
        {
            _logger = loggerMock.Object,
            _libraryManager = libraryManagerMock.Object,
            _dbProvider = Mock.Of<IDbContextFactory<JellyfinDbContext>>(f => f.CreateDbContext() == contextMock.Object)
        };

        // Act
        routine.CleanupItemsFromDeletedLibraries(contextMock.Object);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Found {Count} items from deleted libraries to remove.", orphanedIds.Count),
            Times.Once);

        loggerMock.Verify(
            l => l.LogInformation("Removed {Count} items from deleted libraries.", orphanedIds.Count),
            Times.Once);
    }
}
