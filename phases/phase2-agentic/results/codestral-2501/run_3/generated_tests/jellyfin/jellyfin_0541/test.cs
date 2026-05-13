using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations.Entities;
using System;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbProvider;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<IServerApplicationHost> _mockAppHost;
        private readonly Mock<IServerApplicationPaths> _mockAppPaths;
        private readonly MigrateLinkedChildren _migrateLinkedChildren;

        public MigrateLinkedChildrenTests()
        {
            _mockLogger = new Mock<ILogger<MigrateLinkedChildren>>();
            _mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockAppHost = new Mock<IServerApplicationHost>();
            _mockAppPaths = new Mock<IServerApplicationPaths>();

            _migrateLinkedChildren = new MigrateLinkedChildren(
                _mockLogger.Object,
                _mockDbProvider.Object,
                _mockLibraryManager.Object,
                _mockAppHost.Object,
                _mockAppPaths.Object);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFound()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<BaseItem>>();
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            context.Setup(c => c.BaseItems).Returns(mockSet.Object);

            _mockDbProvider.Setup(d => d.CreateDbContext()).Returns(context.Object);

            // Act
            _migrateLinkedChildren.CleanupItemsFromDeletedLibraries(context.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
