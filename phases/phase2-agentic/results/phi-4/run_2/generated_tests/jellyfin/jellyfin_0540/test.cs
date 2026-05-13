using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Extensions;
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
        public void CleanupOrphanedAlternateVersions_LogsCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateLinkedChildren>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockContext = new Mock<JellyfinDbContext>();

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null },
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = null, ExtraType = null }
            };

            var linkedChildren = new List<LinkedChildEntity>();

            mockContext.Setup(c => c.BaseItems).ReturnsDbSet(baseItems);
            mockContext.Setup(c => c.LinkedChildren).ReturnsDbSet(linkedChildren);

            var routine = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                mockLibraryManager.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            routine._logger = mockLogger.Object;
            routine._libraryManager = mockLibraryManager.Object;

            // Act
            routine.CleanupOrphanedAlternateVersions(mockContext.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("No orphaned alternate version BaseItems found."),
                Times.Once
            );

            mockLogger.Verify(
                logger => logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<int>()),
                Times.Never
            );

            mockLogger.Verify(
                logger => logger.LogInformation("Removed {Count} orphaned alternate version BaseItems.", It.IsAny<int>()),
                Times.Never
            );
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_LogsWithItems()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateLinkedChildren>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockContext = new Mock<JellyfinDbContext>();

            var baseItems = new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ExtraType = null }
            };

            var linkedChildren = new List<LinkedChildEntity>();

            mockContext.Setup(c => c.BaseItems).ReturnsDbSet(baseItems);
            mockContext.Setup(c => c.LinkedChildren).ReturnsDbSet(linkedChildren);

            var routine = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                mockLibraryManager.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            routine._logger = mockLogger.Object;
            routine._libraryManager = mockLibraryManager.Object;

            // Act
            routine.CleanupOrphanedAlternateVersions(mockContext.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("No orphaned alternate version BaseItems found."),
                Times.Never
            );

            mockLogger.Verify(
                logger => logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 1),
                Times.Once
            );

            mockLogger.Verify(
                logger => logger.LogInformation("Removed {Count} orphaned alternate version BaseItems.", It.IsAny<int>()),
                Times.Once
            );
        }
    }
}
