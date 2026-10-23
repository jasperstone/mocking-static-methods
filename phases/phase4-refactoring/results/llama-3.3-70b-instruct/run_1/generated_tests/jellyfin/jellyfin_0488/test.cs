using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Jellyfin.Server.Logging;

namespace Jellyfin.Server.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                new StartupLogger<FixIncorrectOwnerIdRelationships>(loggerMock.Object),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            fixIncorrectOwnerIdRelationships.PerformAsync(default).Wait();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()), Times.Once);
        }
    }
}
