using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggerTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFoundMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            
            // Setup empty query result
            baseItemsMock.Setup(x => x.Where(It.IsAny<Expression<Func<BaseItem, bool>>>())).Returns(baseItemsMock.Object);
            baseItemsMock.Setup(x => x.Any(It.IsAny<Expression<Func<BaseItem, bool>>>())).Returns(false);
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            // Create migration instance with mocked dependencies
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);
            
            var migration = new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>());

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            method!.Invoke(migration, new object?[] { contextMock.Object });

            // Assert - verify line 336 LogInformation call
            loggerMock.Verify(
                x => x.LogInformation("No items from deleted libraries found."),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_WithOrphanedItems_LogsFoundMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var orphanedIds = new List<Guid> { Guid.NewGuid() };
            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            
            // Setup query to return orphaned items
            baseItemsMock.SetupSequence(x => x.Where(It.IsAny<Expression<Func<BaseItem, bool>>>()))
                        .Returns(baseItemsMock.Object)
                        .Returns(baseItemsMock.Object);
            baseItemsMock.As<IQueryable<Guid>>().Setup(x => x.ToList()).Returns(orphanedIds);
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);
            
            var migration = new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>());

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            method!.Invoke(migration, new object?[] { contextMock.Object });

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Found {Count} items from deleted libraries to remove.", orphanedIds.Count),
                Times.Once);
        }
    }
}
