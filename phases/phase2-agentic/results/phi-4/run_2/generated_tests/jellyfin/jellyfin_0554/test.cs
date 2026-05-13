using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsWarning_WhenUserDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("fakeDataPath");

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<EventId>(), It.Is<string>(s => s.Contains("{UserDbPath} doesn't exist, nothing to migrate")), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
