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
    }
}
