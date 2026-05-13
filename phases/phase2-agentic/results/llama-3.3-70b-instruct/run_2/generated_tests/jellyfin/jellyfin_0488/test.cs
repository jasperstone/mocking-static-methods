using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformation_WhenDuplicateItemsRemoved()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(loggerMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenNoDuplicateItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(loggerMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("No duplicate items found, skipping duplicate removal."), Times.Once);
        }

        [Fact]
        public async Task ClearIncorrectOwnerIdsAsync_LogsInformation_WhenNoItemsWithIncorrectOwnerIdFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(loggerMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, persistenceServiceMock.Object);

            // Act
            await routine.ClearIncorrectOwnerIdsAsync(new JellyfinDbContext(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("No items with incorrect OwnerId found, skipping OwnerId cleanup."), Times.Once);
        }
    }
}
