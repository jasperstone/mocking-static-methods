using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines; // Ensure this is correct
using Jellyfin.Database.Implementations; // For JellyfinDbContext
using Jellyfin.Database.Implementations.Entities; // For BaseItemEntity
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
                new BaseItemEntity { Id = orphanedIds[0], TopParentId = Guid.NewGuid(), Type = "SomeType" },
                new BaseItemEntity { Id = orphanedIds[1], TopParentId = Guid.NewGuid(), Type = "SomeType" },
                new BaseItemEntity { Id = orphanedIds[2], TopParentId = Guid.NewGuid(), Type = "SomeType" }
            };

            contextMock.Setup(c => c.BaseItems).Returns(Mock.Of<DbSet<BaseItemEntity>>(b =>
                b.Where(It.IsAny<Func<BaseItemEntity, bool>>()).Returns((Func<BaseItemEntity, bool> predicate) => baseItems.Where(predicate))));

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

    // Mock implementations for testing
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
