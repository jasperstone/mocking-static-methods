using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines; // Assuming this is the correct namespace
using Jellyfin.Database.Implementations; // Assuming this is the correct namespace for JellyfinDbContext
using Jellyfin.Database.Implementations.Entities; // Assuming this is the correct namespace for BaseItemEntity
using Microsoft.EntityFrameworkCore; // For DbSet

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var contextMock = new Mock<JellyfinDbContext>();

            var routine = new MigrateLinkedChildren(
                loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)),
                dbProvider: null,
                libraryManager: libraryManagerMock.Object,
                appHost: null,
                appPaths: null);

            contextMock.Setup(c => c.BaseItems).Returns(Mock.Of<DbSet<BaseItemEntity>>());

            // Act
            routine.CleanupItemsFromDeletedLibraries(contextMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("No items from deleted libraries found."),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_OrphanedItems_LogsCorrectMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var contextMock = new Mock<JellyfinDbContext>();

            var orphanedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = orphanedIds[0], TopParentId = orphanedIds[0], Type = "SomeType" },
                new BaseItemEntity { Id = orphanedIds[1], TopParentId = orphanedIds[1], Type = "SomeType" },
                new BaseItemEntity { Id = orphanedIds[2], TopParentId = orphanedIds[2], Type = "SomeType" }
            };

            contextMock.Setup(c => c.BaseItems).Returns(Mock.Of<DbSet<BaseItemEntity>>(b =>
                b.Where(It.IsAny<Func<BaseItemEntity, bool>>()).Returns(b.Where) &&
                b.Select(It.IsAny<Func<BaseItemEntity, Guid>>()).Returns(b.Select<Guid>()) &&
                b.Any(It.IsAny<Func<BaseItemEntity, bool>>()).Returns(true) &&
                b.ToList().Returns(baseItems)
            ));

            libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((Guid id) => baseItems.FirstOrDefault(b => b.Id == id));

            var routine = new MigrateLinkedChildren(
                loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)),
                dbProvider: null,
                libraryManager: libraryManagerMock.Object,
                appHost: null,
                appPaths: null);

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

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly Mock<ILogger> _loggerMock;

        public MockLoggerProvider(Mock<ILogger> loggerMock)
        {
            _loggerMock = loggerMock;
        }

        public ILogger CreateLogger(string categoryName) => _loggerMock.Object;

        public void Dispose() { }
    }
}
