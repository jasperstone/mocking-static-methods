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

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFound()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>();
            _dbProviderMock.Setup(x => x.CreateDbContext()).Returns(contextMock.Object);

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            var contextMockSet = new Mock<DbSet<BaseItem>>();
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(contextMockSet.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(contextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsItemsFound()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>();
            _dbProviderMock.Setup(x => x.CreateDbContext()).Returns(contextMock.Object);

            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() },
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() }
            }.AsQueryable();

            var contextMockSet = new Mock<DbSet<BaseItem>>();
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(baseItems.Provider);
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(baseItems.Expression);
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(baseItems.ElementType);
            contextMockSet.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(baseItems.GetEnumerator());

            contextMock.Setup(c => c.BaseItems).Returns(contextMockSet.Object);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(contextMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 2 items from deleted libraries to remove.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
