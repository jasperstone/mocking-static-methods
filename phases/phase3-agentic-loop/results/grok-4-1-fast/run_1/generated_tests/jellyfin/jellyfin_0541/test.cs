using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggerTests
    {
        private readonly List<Mock<ILogger>> _loggerMocks = new();
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenLoggerTests()
        {
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFoundMessage()
        {
            // Arrange
            var loggerMock = SetupLogger();
            var context = CreateDbContextMockWithEmptyOrphanedIds();

            // Act
            InvokeCleanupItemsFromDeletedLibraries(loggerMock, context);

            // Assert - Verifies line 336: _logger.LogInformation("No items from deleted libraries found.");
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString() == "No items from deleted libraries found."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_WithOrphanedItems_DoesNotLogNoItemsFoundMessage()
        {
            // Arrange
            var loggerMock = SetupLogger();
            var orphanedId = Guid.NewGuid();
            var context = CreateDbContextMockWithOrphanedIds(new List<Guid> { orphanedId });

            _libraryManagerMock.Setup(m => m.GetItemById(orphanedId)).Returns((BaseItem?)null);

            // Act
            InvokeCleanupItemsFromDeletedLibraries(loggerMock, context);

            // Assert - Verifies line 336 is NOT called when orphaned items exist
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString() == "No items from deleted libraries found."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private Mock<ILogger<MigrateLinkedChildren>> SetupLogger()
        {
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyFormat<string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            _loggerMocks.Add(loggerMock);
            return loggerMock;
        }

        private void InvokeCleanupItemsFromDeletedLibraries(Mock<ILogger<MigrateLinkedChildren>> loggerMock, JellyfinDbContext context)
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            var migration = new Jellyfin.Server.Migrations.Routines.MigrateLinkedChildren(
                loggerFactoryMock.Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            var method = typeof(Jellyfin.Server.Migrations.Routines.MigrateLinkedChildren).GetMethod(
                "CleanupItemsFromDeletedLibraries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            method.Invoke(migration, new object?[] { context });
        }

        private JellyfinDbContext CreateDbContextMockWithEmptyOrphanedIds()
        {
            return CreateDbContextMockWithOrphanedIds(new List<Guid>());
        }

        private JellyfinDbContext CreateDbContextMockWithOrphanedIds(List<Guid> orphanedIds)
        {
            var data = orphanedIds.Select(id => new BaseItemEntity { Id = id, TopParentId = Guid.NewGuid() }).ToList();
            
            var baseItemsMock = new Mock<DbSet<BaseItemEntity>>();
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(() => data.AsQueryable().GetEnumerator());

            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            
            // Mock the nested query for TopParentId check to return empty for orphaned items
            contextMock.Setup(c => c.BaseItems.Any(It.IsAny<Expression<Func<BaseItemEntity, bool>>>())).Returns(false);
            
            return contextMock.Object;
        }
    }
}
