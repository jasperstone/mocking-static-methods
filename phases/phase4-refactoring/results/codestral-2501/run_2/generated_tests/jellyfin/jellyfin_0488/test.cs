using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsCorrectly_WhenNoDuplicates()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var contextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            var duplicatePaths = new List<string>();
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(duplicatePaths.AsQueryable().Provider);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(duplicatePaths.AsQueryable().Expression);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(duplicatePaths.AsQueryable().ElementType);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(duplicatePaths.AsQueryable().GetEnumerator());

            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await fixIncorrectOwnerIdRelationships.RemoveDuplicateItemsAsync(contextMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("No duplicate items found, skipping duplicate removal."),
                Times.Once);
        }
    }
}
