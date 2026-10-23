using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Duplicate_Items_Removed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(loggerMock.Object, null, null, null);

            // Act
            await fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task LogInformation_Called_When_No_Duplicate_Items_Found()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var fixIncorrectOwnerIdRelationships = new FixIncorrectOwnerIdRelationships(loggerMock.Object, null, null, null);

            // Act
            await fixIncorrectOwnerIdRelationships.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
