using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Data;
using Jellyfin.Database.Implementations;
using MediaBrowser.Model.Serialization;
using Microsoft.Data.Sqlite;
using Dapper;

namespace Jellyfin.Server.Migrations.Routines.Tests
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

            var dataPath = "C:\\JellyfinData";
            var userDbPath = Path.Combine(dataPath, "users.db");

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("{UserDbPath} doesn't exist, nothing to migrate", userDbPath),
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

            var dataPath = "C:\\JellyfinData";
            var userDbPath = Path.Combine(dataPath, "users.db");

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Mock the file existence check
            var fileExistsMock = new Mock<File>();
            fileExistsMock.Setup(f => f.Exists(userDbPath)).Returns(true);

            // Mock the database connection and query
            var connectionMock = new Mock<SqliteConnection>();
            connectionMock.Setup(c => c.Open()).Verifiable();
            connectionMock.Setup(c => c.Query("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='LocalUsersv2';"))
                .Returns(new[] { new { count = 0 } });

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Table 'LocalUsersv2' doesn't exist in {UserDbPath}, nothing to migrate", userDbPath),
                Times.Once);
        }
    }
}
