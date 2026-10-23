using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Entities;
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
        public async Task PerformAsync_LogsSuccessfullyRemovedDuplicateEntries()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var duplicatePath = "duplicatePath";

            // Setup BaseItems data with duplicates
            var baseItemsData = new List<BaseItem>
            {
                CreateBaseItem(Guid.NewGuid(), duplicatePath, "MediaBrowser.Controller.Entities.Video", DateTime.UtcNow),
                CreateBaseItem(Guid.NewGuid(), duplicatePath, "MediaBrowser.Controller.Entities.Video", DateTime.UtcNow.AddMinutes(-1))
            }.AsQueryable();

            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItemsData.Provider);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItemsData.Expression);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItemsData.ElementType);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItemsData.GetEnumerator());

            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            // Setup library manager to return items for deletion
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>()))
                .Returns<Guid>(id => CreateBaseItem(id, duplicatePath, "MediaBrowser.Controller.Entities.Video", DateTime.UtcNow));
            libraryManagerMock.Setup(l => l.DeleteItemsUnsafeFast(It.IsAny<IReadOnlyCollection<BaseItem>>()));

            persistenceServiceMock.Setup(p => p.DeleteItem(It.IsAny<IList<Guid>>()));

            var loggedMessages = new List<string>();
            loggerMock.Setup(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Callback<string, object[]>((msg, args) =>
                {
                    var formatted = string.Format(msg.Replace("{Count}", "{0}"), args);
                    loggedMessages.Add(formatted);
                });

            var routine = new FixIncorrectOwnerIdRelationships(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            Assert.Contains(loggedMessages, msg => msg.Contains("Successfully removed"));
        }

        private static BaseItem CreateBaseItem(Guid id, string? path, string? type, DateTime dateCreated)
        {
            var mock = new Mock<BaseItem>();
            mock.SetupGet(b => b.Id).Returns(id);
            mock.SetupGet(b => b.Path).Returns(path);
            mock.SetupGet(b => b.Type).Returns(type);
            mock.SetupGet(b => b.DateCreated).Returns(dateCreated);
            return mock.Object;
        }
    }
}
