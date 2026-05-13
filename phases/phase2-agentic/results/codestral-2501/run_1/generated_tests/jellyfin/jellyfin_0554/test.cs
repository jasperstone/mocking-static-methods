using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Jellyfin.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("/nonexistent/path");

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("{UserDbPath} doesn't exist, nothing to migrate", It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenLocalUsersv2TableDoesNotExist_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("/existing/path");
            var userDbPath = Path.Combine("/existing/path", "users.db");
            File.WriteAllText(userDbPath, ""); // Create an empty file to simulate the existence of the user database

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Table 'LocalUsersv2' doesn't exist in {UserDbPath}, nothing to migrate", It.IsAny<string>()),
                Times.Once);

            // Clean up
            File.Delete(userDbPath);
        }
    }
}
