using Xunit;
using Moq;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task PerformAsync_ShouldLogInformation_WhenDuplicateItemsRemoved()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new JellyfinDbContext(options);
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var migration = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()),
                Times.Once);
        }
    }
}
