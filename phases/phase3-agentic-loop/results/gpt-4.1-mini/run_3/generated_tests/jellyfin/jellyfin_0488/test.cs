using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformationOnDuplicateRemoval()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            // Setup the DbContextFactory to return a mock context
            var contextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock
                .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            // Setup libraryManager and persistenceService mocks to avoid null refs
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((object?)null);
            libraryManagerMock.Setup(l => l.DeleteItemsUnsafeFast(It.IsAny<IList<object>>()));
            persistenceServiceMock.Setup(p => p.DeleteItem(It.IsAny<IList<Guid>>()));

            var migration = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(
                It.Is<string>(s => s.Contains("duplicate database entries")),
                It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
