using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<JellyfinDbContext> _contextMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            
            _contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingCleanupMessage()
        {
            // Arrange
            _dbProviderMock.Setup(x => x.CreateDbContext()).Returns(_contextMock.Object);
            
            var migration = new MigrateLinkedChildren(
                new Mock<ILoggerFactory>().Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Use reflection to call private method
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            // Act
            method.Invoke(migration, new object[] { _contextMock.Object });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedIds_LogsNoItemsFound()
        {
            // Arrange
            SetupNoOrphanedIds();
            
            var migration = CreateMigration();

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            method.Invoke(migration, new object[] { _contextMock.Object });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_LogsStartingCleanupMessage()
        {
            // Arrange
            _dbProviderMock.Setup(x => x.CreateDbContext()).Returns(_contextMock.Object);
            
            var migration = new MigrateLinkedChildren(
                new Mock<ILoggerFactory>().Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);

            // Use reflection to call private method
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupOrphanedAlternateVersions", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            // Act
            method.Invoke(migration, new object[] { _contextMock.Object });

            // Assert - Verifies line 324 specifically
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Starting cleanup of orphaned alternate version BaseItems...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_NoOrphanedVersionIds_LogsNoOrphanedFound()
        {
            // Arrange
            SetupNoOrphanedVersions();
            
            var migration = CreateMigration();

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupOrphanedAlternateVersions", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            method.Invoke(migration, new object[] { _contextMock.Object });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("No orphaned alternate version BaseItems found.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        private MigrateLinkedChildren CreateMigration()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);
            
            _dbProviderMock.Setup(x => x.CreateDbContext()).Returns(_contextMock.Object);
            
            return new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object);
        }

        private void SetupNoOrphanedIds()
        {
            var baseItemsQueryableMock = new Mock<DbSet<BaseItemEntity>>();
            _contextMock.Setup(x => x.BaseItems).Returns(baseItemsQueryableMock.Object);
            
            baseItemsQueryableMock.As<IQueryable<BaseItemEntity>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<BaseItemEntity>(new List<BaseItemEntity>().AsQueryable()));

            baseItemsQueryableMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.Expression)
                .Returns(new List<BaseItemEntity>().AsQueryable().Expression);

            baseItemsQueryableMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.ElementType)
                .Returns(new List<BaseItemEntity>().AsQueryable().ElementType);

            baseItemsQueryableMock.As<IQueryable<BaseItemEntity>>().Setup(m => m.GetEnumerator())
                .Returns(new List<BaseItemEntity>().AsQueryable().GetEnumerator());
        }

        private void SetupNoOrphanedVersions()
        {
            var baseItemsQueryableMock = new Mock<DbSet<BaseItemEntity>>();
            var linkedChildrenQueryableMock = new Mock<DbSet<LinkedChildEntity>>();
            
            _contextMock.Setup(x => x.BaseItems).Returns(baseItemsQueryableMock.Object);
            _contextMock.Setup(x => x.LinkedChildren).Returns(linkedChildrenQueryableMock.Object);
            
            baseItemsQueryableMock.As<IQueryable<BaseItemEntity>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<BaseItemEntity>(new List<BaseItemEntity>().AsQueryable()));

            linkedChildrenQueryableMock.As<IQueryable<LinkedChildEntity>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<LinkedChildEntity>(new List<LinkedChildEntity>().AsQueryable()));
        }
    }
}
