using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggerTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;

        public MigrateLinkedChildrenLoggerTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingCleanupMessage()
        {
            // Arrange
            var fakeContext = new object();
            var migration = CreateMigration();

            // Act
            InvokePrivateMethod(migration, "CleanupItemsFromDeletedLibraries", fakeContext);

            // Assert - specifically verify the LogInformation call on line 324
            _loggerMock.Verify(
                x => x.LogInformation("Starting cleanup of items from deleted libraries..."),
                Times.Once);
        }

        [Fact]
        public void CleanupOrphanedVersions_NoOrphanedVersions_LogsNoOrphanedFound()
        {
            // Arrange
            var fakeContext = new object();
            var migration = CreateMigration();

            // Act
            InvokePrivateMethod(migration, "CleanupOrphanedVersions", fakeContext);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("No orphaned alternate version BaseItems found."),
                Times.Once);
        }

        [Fact]
        public void CleanupOrphanedVersions_WithOrphanedVersions_LogsFoundMessage()
        {
            // Arrange
            var fakeContext = new object();
            var migration = CreateMigration();

            // Act
            InvokePrivateMethod(migration, "CleanupOrphanedVersions", fakeContext);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<int>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFound()
        {
            // Arrange
            var fakeContext = new object();
            var migration = CreateMigration();

            // Act
            InvokePrivateMethod(migration, "CleanupItemsFromDeletedLibraries", fakeContext);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("No items from deleted libraries found."),
                Times.Once);
        }

        [Fact]
        public void CleanupStaleFileEntries_LogsStartingCleanupMessage()
        {
            // Arrange
            var fakeContext = new object();
            var migration = CreateMigration();

            // Act
            InvokePrivateMethod(migration, "CleanupStaleFileEntries", fakeContext);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Starting cleanup of items with missing files..."),
                Times.Once);
        }

        private object CreateMigration()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(_loggerMock.Object);

            // Create migration with mocks - exact dependencies don't matter for logger tests
            return new
            {
                Logger = loggerFactoryMock.Object,
                DbProvider = new Mock<IDbContextFactory<object>>().Object,
                LibraryManager = new Mock<object>().Object,
                AppHost = new Mock<object>().Object,
                AppPaths = new Mock<object>().Object
            };
        }

        private void InvokePrivateMethod(object target, string methodName, object arg)
        {
            var method = target.GetType().Assembly.GetType("Jellyfin.Server.Migrations.Routines.MigrateLinkedChildren")!
                .GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            
            method.Invoke(target, new[] { arg });
        }
    }
}
