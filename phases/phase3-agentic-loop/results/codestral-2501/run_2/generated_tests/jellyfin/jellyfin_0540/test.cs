using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupOrphanedAlternateVersions_LogsCorrectly_WhenNoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            dbProviderMock.Setup(x => x.CreateDbContext()).Returns(dbContextMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                loggerMock.Object,
                dbProviderMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            dbContextMock.Setup(x => x.BaseItems).Returns(baseItems);
            dbContextMock.Setup(x => x.LinkedChildren).Returns(linkedChildren);

            // Act
            migrateLinkedChildren.CleanupOrphanedAlternateVersions(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orphaned alternate version BaseItems found.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_LogsCorrectly_WhenOrphanedItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            dbProviderMock.Setup(x => x.CreateDbContext()).Returns(dbContextMock.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                loggerMock.Object,
                dbProviderMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            var orphanedItemId = Guid.NewGuid();
            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = orphanedItemId, OwnerId = Guid.NewGuid(), ExtraType = null }
            }.AsQueryable();

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();

            dbContextMock.Setup(x => x.BaseItems).Returns(baseItems);
            dbContextMock.Setup(x => x.LinkedChildren).Returns(linkedChildren);

            libraryManagerMock.Setup(x => x.GetItemById(orphanedItemId)).Returns(new BaseItemEntity { Id = orphanedItemId });

            // Act
            migrateLinkedChildren.CleanupOrphanedAlternateVersions(dbContextMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 1 orphaned alternate version BaseItems to remove.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removed 1 orphaned alternate version BaseItems.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
