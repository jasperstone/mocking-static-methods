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
            var userDbPath = "non_existent_path";
            _pathsMock.Setup(p => p.DataPath).Returns("data_path");
            _pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns("user_config_path");

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
