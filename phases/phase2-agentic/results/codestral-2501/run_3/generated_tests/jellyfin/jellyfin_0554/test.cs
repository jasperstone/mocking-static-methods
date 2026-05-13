using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
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
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var dataPath = "path/to/data";
            var userDbPath = Path.Combine(dataPath, "users.db");
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var migrateUserDb = new MigrateUserDb(
                _loggerMock.Object,
                _pathsMock.Object,
                _providerMock.Object,
                _xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("{UserDbPath} doesn't exist, nothing to migrate", userDbPath),
                Times.Once);
        }
    }
}
