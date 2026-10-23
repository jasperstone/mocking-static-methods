using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using System.IO;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_WhenFileMoveFails_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            var dataPath = "testDataPath";
            var dbFilename = "users.db";
            var userDbPath = Path.Combine(dataPath, dbFilename);

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            // Simulate the file not existing
            if (File.Exists(userDbPath))
            {
                File.Delete(userDbPath);
            }

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
