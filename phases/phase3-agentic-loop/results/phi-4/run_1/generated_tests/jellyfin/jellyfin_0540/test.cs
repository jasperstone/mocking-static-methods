using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Server.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly MigrateLinkedChildren _migrateLinkedChildren;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            _migrateLinkedChildren = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                _dbContextFactoryMock.Object,
                _libraryManagerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_NoOrphanedVersionsFound_LogsCorrectly()
        {
            // Arrange
            var context = new JellyfinDbContext(new DbContextOptions<JellyfinDbContext>());
            context.BaseItems.AddRange(new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = Guid.NewGuid(), OwnerId = null, ExtraType = null }
            });
            context.SaveChanges();

            _dbContextFactoryMock.Setup(df => df.CreateDbContext()).Returns(context);

            // Act
            _migrateLinkedChildren.CleanupOrphanedAlternateVersions(context);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("No orphaned alternate version BaseItems found."),
                Times.Once
            );
            _loggerMock.Verify(
                logger => logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<int>()),
                Times.Never
            );
            _loggerMock.Verify(
                logger => logger.LogInformation("Removed {Count} orphaned alternate version BaseItems.", It.IsAny<int>()),
                Times.Never
            );
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_OrphanedVersionsFound_LogsCorrectly()
        {
            // Arrange
            var context = new JellyfinDbContext(new DbContextOptions<JellyfinDbContext>());
            var orphanedId = Guid.NewGuid();
            context.BaseItems.AddRange(new List<BaseItemEntity>
            {
                new BaseItemEntity { Id = orphanedId, OwnerId = orphanedId, ExtraType = null }
            });
            context.SaveChanges();

            _dbContextFactoryMock.Setup(df => df.CreateDbContext()).Returns(context);

            _libraryManagerMock
                .Setup(lm => lm.GetItemById(orphanedId))
                .Returns(new BaseItemEntity { Id = orphanedId });

            // Act
            _migrateLinkedChildren.CleanupOrphanedAlternateVersions(context);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation("No orphaned alternate version BaseItems found."),
                Times.Never
            );
            _loggerMock.Verify(
                logger => logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 1),
                Times.Once
            );
            _loggerMock.Verify(
                logger => logger.LogInformation("Removed {Count} orphaned alternate version BaseItems.", 1),
                Times.Once
            );
        }
    }
}
