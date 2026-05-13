using System;
using System.IO;
using System.Linq;
using Jellyfin.Data;
using Jellyfin.Server.Interfaces;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _providerMock;
        private readonly Mock<IXmlSerializer> _xmlSerializerMock;

        public MigrateUserDbTests()
        {
            _loggerMock = new Mock<ILogger<MigrateUserDb>>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _xmlSerializerMock = new Mock<IXmlSerializer>();
        }

        [Fact]
        public void Perform_UserDbDoesNotExist_LogsWarningAndReturns()
        {
            // Arrange
            var dataPath = "/path/to/data";
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");
            _pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns("/path/to/user/config");

            var migrateUserDb = new MigrateUserDb(_loggerMock.Object, _pathsMock.Object, _providerMock.Object, _xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(userDbPath))), Times.Once);
        }

        [Fact]
        public void Perform_TableLocalUsersv2DoesNotExist_LogsWarningAndReturns()
        {
            // Arrange
            var dataPath = "/path/to/data";
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");
            _pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns("/path/to/user/config");

            using var connection = new SqliteConnection($"Filename={userDbPath}");
            connection.Open();

            var migrateUserDb = new MigrateUserDb(_loggerMock.Object, _pathsMock.Object, _providerMock.Object, _xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Table 'LocalUsersv2' doesn't exist"))), Times.Once);
        }
    }
}
