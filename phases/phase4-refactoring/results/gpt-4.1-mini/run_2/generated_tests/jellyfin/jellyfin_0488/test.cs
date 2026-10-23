using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformationOnNoDuplicates()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            // Setup DbContextFactory to return a dummy context that has no duplicates
            var contextMock = new Mock<JellyfinDbContext>();
            // We do not setup BaseItems because no duplicates means no paths with count > 1

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("No duplicate items found, skipping duplicate removal."), Times.Once);
        }
    }
}
