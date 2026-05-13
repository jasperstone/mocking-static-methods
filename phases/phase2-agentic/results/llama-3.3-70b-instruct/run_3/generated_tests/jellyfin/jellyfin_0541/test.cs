using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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
            var migrateLinkedChildren = new MigrateLinkedChildren(loggerMock.Object, null, null, null, null);

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("No items from deleted libraries found."), Times.Once);
        }

        [Fact]
        public void LogInformation_CalledWithCorrectMessage_WhenItemsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var migrateLinkedChildren = new MigrateLinkedChildren(loggerMock.Object, null, null, null, null);
            var contextMock = new Mock<JellyfinDbContext>();
            contextMock.Setup(c => c.BaseItems.Where(It.IsAny<Func<BaseItem, bool>>()))
                .Returns(new List<BaseItem> { new BaseItem { Id = 1 } });

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(contextMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Found {Count} items from deleted libraries to remove.", 1), Times.Once);
        }
    }
}
