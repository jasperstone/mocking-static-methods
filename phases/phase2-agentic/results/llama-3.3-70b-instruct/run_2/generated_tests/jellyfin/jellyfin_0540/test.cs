using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Server.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenOrphanedVersionIdsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );
            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(new List<BaseItem>
            {
                new BaseItem { Id = 1, OwnerId = 1, ExtraType = null },
                new BaseItem { Id = 2, OwnerId = 2, ExtraType = null },
            }.AsQueryable());

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenNoOrphanedVersionIdsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>()
            );
            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems).Returns(new List<BaseItem>().AsQueryable());

            // Act
            migrateLinkedChildren.Perform();

            // Assert
            loggerMock.Verify(l => l.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
        }
    }
}
