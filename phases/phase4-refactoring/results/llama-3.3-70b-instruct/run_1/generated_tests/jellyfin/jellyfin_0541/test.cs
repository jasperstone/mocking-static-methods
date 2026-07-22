using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Jellyfin.Server.Migrations.Routines;

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
                null,
                null,
                null,
                null);

            // Act
            migrateLinkedChildren._logger = loggerMock.Object;
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("No items from deleted libraries found."), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenOrphanedItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory(),
                null,
                null,
                null,
                null);

            // Act
            migrateLinkedChildren._logger = loggerMock.Object;
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} items from deleted libraries to remove.", It.IsAny<object>()), Times.Once);
        }
    }
}
