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
        public async Task RemoveDuplicateItemsAsync_LogsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockPersistenceService = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(
                new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>().Object,
                mockDbContextFactory.Object,
                mockLibraryManager.Object,
                mockPersistenceService.Object);

            var mockContext = new Mock<JellyfinDbContext>();
            mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockContext.Object);

            var duplicatePaths = new List<string> { "/path/to/duplicate1", "/path/to/duplicate2" };
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate1" },
                new BaseItem { Id = Guid.NewGuid(), Path = "/path/to/duplicate2" }
            };

            mockContext.Setup(c => c.BaseItems)
                .ReturnsDbSet(baseItems);

            // Act
            await routine.RemoveDuplicateItemsAsync(mockContext.Object, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully removed 2 duplicate database entries")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
