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

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbContextFactory;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<IItemPersistenceService> _mockPersistenceService;
        private readonly FixIncorrectOwnerIdRelationships _fixIncorrectOwnerIdRelationships;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _mockLogger = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            _mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockPersistenceService = new Mock<IItemPersistenceService>();
            _fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(
                _mockLogger.Object,
                _mockDbContextFactory.Object,
                _mockLibraryManager.Object,
                _mockPersistenceService.Object);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenDuplicateItemsRemoved()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", DateCreated = DateTime.Now },
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", DateCreated = DateTime.Now.AddDays(-1) }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<BaseItem>>();
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            context.Setup(c => c.BaseItems).Returns(mockSet.Object);
            _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(context.Object);

            // Act
            await _fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenNoDuplicateItemsFound()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", DateCreated = DateTime.Now }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<BaseItem>>();
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            context.Setup(c => c.BaseItems).Returns(mockSet.Object);
            _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(context.Object);

            // Act
            await _fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenDuplicateItemsRemoved()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", DateCreated = DateTime.Now },
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", DateCreated = DateTime.Now.AddDays(-1) }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<BaseItem>>();
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            context.Setup(c => c.BaseItems).Returns(mockSet.Object);
            _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(context.Object);

            // Act
            await _fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenNoDuplicateItemsFound()
        {
            // Arrange
            var context = new Mock<JellyfinDbContext>();
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), Path = "path1", DateCreated = DateTime.Now }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<BaseItem>>();
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            mockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            context.Setup(c => c.BaseItems).Returns(mockSet.Object);
            _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(context.Object);

            // Act
            await _fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.LogInformation(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()),
                Times.Once);
        }
    }

    public class BaseItem
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid? OwnerId { get; set; }
        public int? ExtraType { get; set; }
        public string Type { get; set; }
        public Guid? ParentId { get; set; }
    }
}
