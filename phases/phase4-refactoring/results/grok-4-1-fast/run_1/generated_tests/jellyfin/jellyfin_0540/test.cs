using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggerTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenLoggerTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingCleanupMessage()
        {
            // Arrange
            SetupDbContextWithNoOrphanedItems();
            
            var migration = CreateMigration();
            var method = GetCleanupMethod();

            // Act
            method.Invoke(migration, new[] { _dbProviderMock.Object.CreateDbContext() });

            // Assert - Verifies the LogInformation call on line 324
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((IReadOnlyList<KeyValuePair<string, object?>>?)v)?.Any(kvp => 
                            kvp.Value?.ToString()?.Contains("Starting cleanup of items from deleted libraries") == true) == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFound()
        {
            // Arrange
            SetupDbContextWithNoOrphanedItems();
            
            var migration = CreateMigration();
            var method = GetCleanupMethod();

            // Act
            method.Invoke(migration, new[] { _dbProviderMock.Object.CreateDbContext() });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((IReadOnlyList<KeyValuePair<string, object?>>?)v)?.Any(kvp => 
                            kvp.Value?.ToString()?.Contains("No items from deleted libraries found.") == true) == true),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetupDbContextWithNoOrphanedItems()
        {
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<DbSet<BaseItemEntity>>();
            
            // Setup empty orphaned results
            var emptyQuery = Enumerable.Empty<BaseItemEntity>().AsQueryable();
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Provider).Returns(emptyQuery.Provider);
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression).Returns(emptyQuery.Expression);
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType).Returns(emptyQuery.ElementType);
            baseItemsMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator()).Returns(() => emptyQuery.GetEnumerator());
            
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(contextMock.Object);
        }

        private MigrateLinkedChildren CreateMigration()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);
            
            return new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);
        }

        private static MethodInfo GetCleanupMethod()
        {
            return typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        }
    }
}
