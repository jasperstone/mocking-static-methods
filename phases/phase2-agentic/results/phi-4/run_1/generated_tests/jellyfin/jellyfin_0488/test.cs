using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task PerformAsync_LogsCorrectMessage_WhenDuplicatesAreRemoved()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(
                new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>().Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            var contextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock
                .Setup(factory => factory.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            var duplicatePaths = new List<string> { "/path/to/duplicate1", "/path/to/duplicate2" };
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate2" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate2" }
            };

            contextMock
                .Setup(ctx => ctx.BaseItems)
                .ReturnsDbSet(baseItems);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                log => log.LogInformation(
                    "Successfully removed {Count} duplicate database entries",
                    It.Is<int>(count => count == 3)),
                Times.Once);
        }
    }
}
