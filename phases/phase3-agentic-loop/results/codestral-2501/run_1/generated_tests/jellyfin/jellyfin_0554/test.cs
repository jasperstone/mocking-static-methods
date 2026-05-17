using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IXmlSerializer> _xmlSerializerMock;

        public MigrateUserDbTests()
        {
            _loggerMock = new Mock<ILogger<MigrateUserDb>>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _xmlSerializerMock = new Mock<IXmlSerializer>();
        }

        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var dataPath = "path/to/data";
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");
            if (File.Exists(userDbPath))
            {
                File.Delete(userDbPath);
            }

            var migrateUserDb = new MigrateUserDb(
                _loggerMock.Object,
                _pathsMock.Object,
                null,
                _xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("{UserDbPath} doesn't exist, nothing to migrate")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenLocalUsersv2TableDoesNotExist_LogsWarning()
        {
            // Arrange
            var dataPath = "path/to/data";
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");
            if (!File.Exists(userDbPath))
            {
                using var connection = new SqliteConnection($"Filename={userDbPath}");
                connection.Open();
                connection.Execute("CREATE TABLE sqlite_master (type TEXT, name TEXT);");
            }

            var migrateUserDb = new MigrateUserDb(
                _loggerMock.Object,
                _pathsMock.Object,
                null,
                _xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Table 'LocalUsersv2' doesn't exist in {UserDbPath}, nothing to migrate")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
