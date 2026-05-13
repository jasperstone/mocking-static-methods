using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly JellyfinDbContext _contextMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();

            _contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingCleanupMessage()
        {
            // Arrange
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(_contextMock.Object);
            var migration = CreateMigration();

            // Act
            migration.PerformCleanupItemsFromDeletedLibraries(_contextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFound()
        {
            // Arrange
            SetupNoOrphanedLibraryItems();
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(_contextMock.Object);
            var migration = CreateMigration();

            // Act
            migration.PerformCleanupItemsFromDeletedLibraries(_contextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_WithOrphanedItems_LogsFoundAndRemovedMessages()
        {
            // Arrange
            var orphanedId = Guid.NewGuid();
            SetupOrphanedLibraryItems(orphanedId);
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(_contextMock.Object);
            var migration = CreateMigration();

            // Act
            migration.PerformCleanupItemsFromDeletedLibraries(_contextMock.Object);

            // Assert - Verify the specific LogInformation call on line 324
            _loggerMock.Verify(
                x => x.LogInformation("Starting cleanup of items from deleted libraries..."),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation("Found {Count} items from deleted libraries to remove.", 1),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation("Removed {Count} items from deleted libraries.", 1),
                Times.Once);
        }

        private void SetupNoOrphanedLibraryItems()
        {
            _contextMock.Setup(c => c.BaseItems).Returns(new TestDbSet<BaseItemEntity>());
            // No orphaned items scenario - query returns empty
        }

        private void SetupOrphanedLibraryItems(Guid orphanedId)
        {
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<BaseItemEntity>>();
            _contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            // Setup orphaned items query to return one orphaned item
            baseItemsMock.Setup(b => b.Where(It.IsAny<Func<BaseItemEntity, bool>>()))
                .Returns(baseItemsMock.Object);

            baseItemsMock.Setup(b => b.Select(It.IsAny<Func<BaseItemEntity, Guid>>()))
                .Returns(new[] { orphanedId }.AsQueryable());
        }

        private MigrateLinkedChildren CreateMigration()
        {
            return new MigrateLinkedChildren(
                new Mock<ILoggerFactory>().Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);
        }
    }
}
