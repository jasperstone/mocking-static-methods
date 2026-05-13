using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Model.Serialization;
using Jellyfin.Data;
using System;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _providerMock;
        private readonly Mock<IXmlSerializer> _xmlSerializerMock;
        private readonly MigrateUserDb _migrateUserDb;

        public MigrateUserDbTests()
        {
            _loggerMock = new Mock<ILogger<MigrateUserDb>>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _xmlSerializerMock = new Mock<IXmlSerializer>();

            _migrateUserDb = new MigrateUserDb(
                _loggerMock.Object,
                _pathsMock.Object,
                _providerMock.Object,
                _xmlSerializerMock.Object);
        }

        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            _pathsMock.Setup(p => p.DataPath).Returns("path/to/data");
            var userDbPath = Path.Combine("path/to/data", "users.db");
            if (File.Exists(userDbPath))
            {
                File.Delete(userDbPath);
            }

            // Act
            _migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("doesn't exist, nothing to migrate")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenLocalUsersv2TableDoesNotExist_LogsWarning()
        {
            // Arrange
            _pathsMock.Setup(p => p.DataPath).Returns("path/to/data");
            var userDbPath = Path.Combine("path/to/data", "users.db");
            if (!File.Exists(userDbPath))
            {
                File.Create(userDbPath).Close();
            }

            // Act
            _migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Table 'LocalUsersv2' doesn't exist")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenIOExceptionOccurs_LogsError()
        {
            // Arrange
            _pathsMock.Setup(p => p.DataPath).Returns("path/to/data");
            var userDbPath = Path.Combine("path/to/data", "users.db");
            if (!File.Exists(userDbPath))
            {
                File.Create(userDbPath).Close();
            }

            // Act
            _migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
