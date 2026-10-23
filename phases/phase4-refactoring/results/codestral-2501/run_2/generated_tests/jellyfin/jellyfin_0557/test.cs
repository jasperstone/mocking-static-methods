using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_ShouldLogError_WhenRenamingLegacyUserDatabaseFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var dataPath = "testDataPath";
            var dbFilename = "users.db";
            var userDbPath = Path.Combine(dataPath, dbFilename);

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Simulate the scenario where renaming the legacy user database fails
            File.Create(userDbPath).Close();
            File.Create(Path.Combine(dataPath, dbFilename + "-journal")).Close();

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
