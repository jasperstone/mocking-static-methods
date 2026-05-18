using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using System.IO;
using MediaBrowser.Controller;
using Jellyfin.Data;
using Jellyfin.Database.Implementations;
using MediaBrowser.Model.Serialization;
using Microsoft.EntityFrameworkCore;
using System;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_WhenRenamingLegacyUserDatabaseFails()
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

            // Simulate the scenario where the file move operation fails
            File.WriteAllText(userDbPath, "dummy content");
            File.Move(userDbPath, Path.Combine(dataPath, dbFilename + ".old"));

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
