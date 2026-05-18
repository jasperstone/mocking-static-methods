using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Data;

namespace Jellyfin.Server.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsWarning_WhenUserDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<Data.JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<MediaBrowser.Model.Serialization.IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void Perform_LogsWarning_WhenTableDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<Data.JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<MediaBrowser.Model.Serialization.IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var userDbPath = Path.Combine("DataPath", "users.db");
            File.Create(userDbPath).Dispose();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            File.Delete(userDbPath);
        }

        [Fact]
        public void Perform_MigratesUser_WhenUserDbAndTableExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<Data.JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<MediaBrowser.Model.Serialization.IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var userDbPath = Path.Combine("DataPath", "users.db");
            File.Create(userDbPath).Dispose();

            using (var connection = new System.Data.SQLite.SQLiteConnection($"Filename={userDbPath}"))
            {
                connection.Open();
                connection.Execute("CREATE TABLE LocalUsersv2 (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
                connection.Execute("INSERT INTO LocalUsersv2 (Id, Name) VALUES (1, 'TestUser')");
            }

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            using (var connection = new System.Data.SQLite.SQLiteConnection($"Filename={userDbPath}"))
            {
                connection.Open();
                var result = connection.Query("SELECT * FROM LocalUsersv2");
                Assert.Single(result);
            }
            File.Delete(userDbPath);
        }
    }
}
