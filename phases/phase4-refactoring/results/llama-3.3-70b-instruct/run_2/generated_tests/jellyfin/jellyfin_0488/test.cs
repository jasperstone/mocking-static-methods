using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var logger = new Microsoft.Extensions.Logging.Logger<FixIncorrectOwnerIdRelationships>(loggerMock.Object);
            var dbContextFactoryMock = new Mock<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                new Jellyfin.Server.Logging.StartupLogger<FixIncorrectOwnerIdRelationships>(loggerMock.Object),
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            var contextMock = new Mock<Jellyfin.Database.Implementations.JellyfinDbContext>();
            fixIncorrectOwnerIdRelationships.PerformAsync(default).Wait();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<object[]>()), Times.Once);
        }
    }
}
