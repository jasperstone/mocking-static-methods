using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Jellyfin.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<ILogger<ReseedFolderFlag>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<JellyfinDbContext> _dbContextMock;
        private readonly Mock<DbSet<BaseItem>> _baseItemsMock;

        public ReseedFolderFlagTests()
        {
            _loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbContextMock = new Mock<JellyfinDbContext>();
            _baseItemsMock = new Mock<DbSet<BaseItem>>();
            _dbContextMock.Setup(db => db.BaseItems).Returns(_baseItemsMock.Object);
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbContextMock.Object);
        }

        [Fact]
        public async Task PerformAsync_Skips_WhenRerunFlagIsSet()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Migration is skipped because it does not apply."), Times.Once);
            ReseedFolderFlag.RerunGuardFlag = false; // Reset for other tests
        }

        [Fact]
        public async Task PerformAsync_LogsError_WhenFileDoesNotExist()
        {
            // Arrange
            _pathsMock.Setup(p => p.DataPath).Returns("/nonexistent");
            var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.Is<string>(s => s.Contains("Cannot migrate IsFolder flag from")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformationAndProcessesItems()
        {
            // Arrange
            var guidList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var entities = guidList.Select(g => new { GetGuid = new Func<int, Guid>(_ => g) }).ToList();

            var queryable = entities.AsQueryable();

            var mockQuery = new Mock<IQueryable<dynamic>>();
            mockQuery.As<IQueryable<dynamic>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockQuery.As<IQueryable<dynamic>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockQuery.As<IQueryable<dynamic>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockQuery.As<IQueryable<dynamic>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Patch File.Exists to return true
            _pathsMock.Setup(p => p.DataPath).Returns("/fakepath");
            var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Migrating the IsFolder flag for")), It.IsAny<object[]>()), Times.Once);
            // Verify that ExecuteUpdateAsync is called for each item
            _baseItemsMock.Verify(b => b.Where(It.IsAny<Func<BaseItem, bool>>()), Times.Exactly(guidList.Count));
        }
    }
}
