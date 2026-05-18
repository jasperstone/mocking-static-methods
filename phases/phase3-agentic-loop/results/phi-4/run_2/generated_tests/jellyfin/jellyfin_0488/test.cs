using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Persistence;
using Jellyfin.Server.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenAllIdsToDeleteCountIsGreaterThanZero()
        {
            // Arrange
            var mockLogger = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockPersistenceService = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(
                mockLogger.Object,
                mockDbContextFactory.Object,
                mockLibraryManager.Object,
                mockPersistenceService.Object);

            var cancellationToken = CancellationToken.None;

            // Act
            routine.ExecuteAsync(cancellationToken).Wait();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message == "Successfully removed {Count} duplicate database entries"),
                    It.Is<int>(count => count > 0)),
                Times.Once);
        }

        [Fact]
        public void LogInformation_ShouldNotBeCalled_WhenAllIdsToDeleteCountIsZero()
        {
            // Arrange
            var mockLogger = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockPersistenceService = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(
                mockLogger.Object,
                mockDbContextFactory.Object,
                mockLibraryManager.Object,
                mockPersistenceService.Object);

            var cancellationToken = CancellationToken.None;

            // Act
            routine.ExecuteAsync(cancellationToken).Wait();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(message => message == "Successfully removed {Count} duplicate database entries"),
                    It.Is<int>(count => count > 0)),
                Times.Never);
        }
    }
}
