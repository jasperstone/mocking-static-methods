using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private readonly Mock<IServerApplicationPaths> _mockPaths;
        private readonly Mock<object> _mockDbFactory;
        private readonly Mock<IXmlSerializer> _mockXmlSerializer;
        private readonly Mock<ILogger<MigrateUserDb>> _mockLogger;

        public MigrateUserDbTests()
        {
            _mockPaths = new Mock<IServerApplicationPaths>();
            _mockDbFactory = new Mock<object>();
            _mockXmlSerializer = new Mock<IXmlSerializer>();
            _mockLogger = new Mock<ILogger<MigrateUserDb>>();
        }

        [Fact]
        public void Perform_UserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var dataPath = "/test/data";
            var userDbPath = Path.Combine(dataPath, "users.db");
            _mockPaths.Setup(p => p.DataPath).Returns(dataPath);
            
            var migration = new MigrateUserDb(
                _mockLogger.Object,
                _mockPaths.Object,
                (global::IDbContextFactory<Jellyfin.Data.JellyfinDbContext>)_mockDbFactory.Object,
                _mockXmlSerializer.Object);

            // Act
            migration.Perform();

            // Assert
            _mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "{UserDbPath} doesn't exist, nothing to migrate",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
