using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;

namespace Jellyfin.Server.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task ClearIncorrectOwnerIdsAsync_LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            var contextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(df => df.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            // Act
            await fixIncorrectOwnerIdRelationships.ClearIncorrectOwnerIdsAsync(contextMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("No items with incorrect OwnerId found, skipping OwnerId cleanup."), Times.Once);
        }
    }
}
